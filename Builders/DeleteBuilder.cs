using Cassandra;
using CassandraORM.Tokens;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CassandraORM.Builders
{
    public class DeleteBuilder<T> : QueryBuilder<DeleteQuery, T>
    {
        public DeleteBuilder<T> UseTimestamp(long value)
        {
            CurrentQuery.UpdateParameters.Add(new UpdateParameter(UpdateParameter.UpdateParameterTypes.Timestamp, GetRestrictedValue(value)));

            return this;
        }

        public DeleteBuilder<T> UseTTL(long value)
        {
            CurrentQuery.UpdateParameters.Add(new UpdateParameter(UpdateParameter.UpdateParameterTypes.TTL, GetRestrictedValue(value)));

            return this;
        }

        public DeleteBuilder<T> Delete(params Expression<Func<T, object>>[] selects)
        {
            if (selects != null)
            {
                foreach (Expression<Func<T, object>> select in selects)
                {
                    SimpleSelection selection = BuilderHelper.ParseSimpleSelection(select);
                    
                    if(selection != null)
                    {
                        CurrentQuery.Selections.Add(selection);
                    }
                }
            }

            return this;
        }

        public DeleteBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            CurrentQuery.WhereClause = BuilderHelper.ParseWhere(predicate);

            return this;
        }

        public DeleteBuilder<T> If(Expression<Func<T, bool>> predicate)
        {
            CurrentQuery.Conditions = BuilderHelper.ParseConditions(predicate);

            return this;
        }

        public bool Execute()
        {
            RowSet rowSet = Session.Execute(BuildQuery());
            Row row = rowSet.FirstOrDefault();

            return row == null ? false : row[0] as bool? ?? false;
        }

        public async Task<bool> ExecuteAsync()
        {
            RowSet rowSet = await Session.ExecuteAsync(BuildQuery());
            Row row = rowSet.FirstOrDefault();

            return row == null ? false : row[0] as bool? ?? false;
        }

        private IRestrictedValue GetRestrictedValue(long value)
        {
            if (BuilderHelper.UseBindings)
            {
                string name = GetDistinctAlias();

                Parameters.Add(name, value);

                return new BindMarker()
                {
                    Identifier = name
                };
            }
            else
            {
                return new IntegerConstant() { Value = value };
            }
        }
    }
}
