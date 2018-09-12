using Cassandra;
using CassandraORM.Cache;
using CassandraORM.Tokens;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CassandraORM.Builders
{
    public class InsertBuilder<T> : QueryBuilder<InsertQuery, T>
    {
        public InsertBuilder<T> UseTimestamp(long timestamp)
        {
            UpdateParameter parameter = new UpdateParameter(UpdateParameter.UpdateParameterTypes.Timestamp, GetParameterValue(timestamp));

            CurrentQuery.UpdateParameters.Add(parameter);

            return this;
        }

        public InsertBuilder<T> UseTTL(long ttl)
        {
            UpdateParameter parameter = new UpdateParameter(UpdateParameter.UpdateParameterTypes.TTL, GetParameterValue(ttl));

            CurrentQuery.UpdateParameters.Add(parameter);

            return this;
        }

        public InsertBuilder<T> IfNotExists()
        {
            CurrentQuery.IfNotExists = true;

            return this;
        }

        public bool Insert(T item)
        {
            CurrentQuery.InsertedData = GetNamesClause(item);

            return Execute();
        }

        public InsertBuilder<T> SetItem(T item)
        {
            CurrentQuery.InsertedData = GetNamesClause(item);

            return this;
        }
        
        public async Task<bool> InsertAsync(T item)
        {
            CurrentQuery.InsertedData = GetNamesClause(item);

            return await ExecuteAsync();
        }

        public bool InsertJson(string json, JsonClause.JsonDefaults defaults = JsonClause.JsonDefaults.None)
        {
            CurrentQuery.InsertedData = new JsonClause()
            {
                Json = GenerateBindMarker(json),
                Default = defaults
            };

            return Execute();
        }

        public async Task<bool> InsertJsonAsync(string json, JsonClause.JsonDefaults defaults = JsonClause.JsonDefaults.None)
        {
            JsonClause clause = new JsonClause()
            {
                Json = GenerateBindMarker(json),
                Default = defaults
            };

            CurrentQuery.InsertedData = clause;

            return await ExecuteAsync();
        }

        public bool Execute()
        {
            RowSet rows = Session.Execute(BuildQuery());
            Row row = rows.FirstOrDefault();

            return row == null ? false : row[0] as bool? ?? false;
        }

        public async Task<bool> ExecuteAsync()
        {
            RowSet rows = await Session.ExecuteAsync(BuildQuery());
            Row row = rows.FirstOrDefault();

            return row == null ? false : row[0] as bool? ?? false;
        }

        private NamesClause GetNamesClause(T item)
        {
            NamesClause clause = new NamesClause();

            List<Term> tupleTerms = new List<Term>();

            foreach (KeyValuePair<string, PropertyInfo> mappedProperty in TableCache<T>.CachedTable.Columns)
            {
                object value = mappedProperty.Value.GetValue(item);

                if (value != null)
                {
                    tupleTerms.Add(GenerateBindMarker(value));
                    clause.ColumnNames.Add(mappedProperty.Key);
                }
            }

            clause.Values = new TupleLiteral()
            {
                Terms = tupleTerms
            };

            return clause;
        }

        private IRestrictedValue GetParameterValue(long parameterValue)
        {
            IRestrictedValue value = null;

            if (BuilderHelper.UseBindings)
            {
                string name = GetDistinctAlias();

                value = new BindMarker()
                {
                    Identifier = name
                };

                Parameters.Add(name, parameterValue);
            }
            else
            {
                value = new IntegerConstant()
                {
                    Value = parameterValue
                };
            }

            return value;
        }

        private BindMarker GenerateBindMarker(object value)
        {
            string name = GetDistinctAlias();

            Parameters.Add(name, value);

            return new BindMarker()
            {
                Identifier = name
            };
        }
    }
}
