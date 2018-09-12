using Cassandra;
using CassandraORM.Cache;
using CassandraORM.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CassandraORM.Builders
{
    public class SelectBuilder<T> : QueryBuilder<SelectQuery, T>
    {
        private IEnumerable<string> _selectedFields;
        
        public SelectBuilder<T> AllowFiltering()
        {
            CurrentQuery.AllowFiltering = true;

            return this;
        }

        public SelectBuilder<T> LimitPerPartition(long limit)
        {
            if (BuilderHelper.UseBindings)
            {
                string name = GetDistinctAlias();

                CurrentQuery.PerPartitionLimit = new BindMarker()
                {
                    Identifier = name,
                };

                Parameters.Add(name, limit);
            }
            else
            {
                CurrentQuery.PerPartitionLimit = new IntegerConstant() { Value = limit };
            }

            return this;
        }

        public SelectBuilder<T> Limit(long limit)
        {
            if (BuilderHelper.UseBindings)
            {
                string name = GetDistinctAlias();

                CurrentQuery.Limit = new BindMarker()
                {
                    Identifier = name
                };

                Parameters.Add(name, limit);
            }
            else
            {
                CurrentQuery.Limit = new IntegerConstant { Value = limit };
            }

            return this;
        }

        public SelectBuilder<T> Json()
        {
            CurrentQuery.Json = true;

            return this;
        }

        public SelectBuilder<T> Distinct()
        {
            CurrentQuery.Distinct = true;

            return this;
        }

        public SelectBuilder<T> Select(params Expression<Func<T, object>>[] selectExpressions)
        {
            if (selectExpressions != null)
            {
                _selectedFields = Enumerable.Empty<string>();
                KeyValuePair<Select, string>[] selects = new KeyValuePair<Select, string>[selectExpressions.Length];

                for (int i = 0; i < selects.Length; i++)
                {
                    selects[i] = BuilderHelper.ParseAliasedSelect(selectExpressions[i]);

                    if (string.IsNullOrWhiteSpace(selects[i].Value))
                    {
                        StringBuilder columnBuilder = new StringBuilder();

                        selects[i].Key.WriteCql(columnBuilder);

                        _selectedFields = _selectedFields.Append(columnBuilder.ToString());
                    }
                    else
                    {
                        _selectedFields = _selectedFields.Append(selects[i].Value);
                    }
                }

                CurrentQuery.SelectClause = new SelectClause()
                {
                    Selects = selects
                };
            }

            return this;
        }

        public SelectBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            CurrentQuery.WhereClause = BuilderHelper.ParseWhere(predicate);

            return this;
        }

        public SelectBuilder<T> GroupBy(params Expression<Func<T, object>>[] columnNames)
        {
            List<string> columns = new List<string>();

            foreach (Expression<Func<T, object>> columnName in columnNames)
            {
                Expression current = columnName?.Body;

                if(current == null)
                {
                    continue;
                }

                if (current is UnaryExpression unaryExpression &&
                   (unaryExpression.NodeType == ExpressionType.Convert ||
                    unaryExpression.NodeType == ExpressionType.ConvertChecked))
                {
                    current = unaryExpression.Operand;
                }

                if (current is MemberExpression member)
                {
                    columns.Add(TableCache<T>.CachedTable.Aliases[member.Member.Name]);
                }
            }

            CurrentQuery.GroupingClause = new GroupingClause()
            {
                FieldNames = columns
            };

            return this;
        }

        public SelectBuilder<T> OrderBy(Expression<Func<T, object>> column)
        {
            AddOrder(column, Order.OrderTypes.Ascending);

            return this;
        }

        public SelectBuilder<T> OrderByDescending(Expression<Func<T, object>> column)
        {
            AddOrder(column, Order.OrderTypes.Descending);

            return this;
        }

        public T FirstOrDefault()
        {
            CurrentQuery.Limit = new IntegerConstant() { Value = 1 };
            RowSet results = Session.Execute(BuildQuery());
            Row first = results.FirstOrDefault();

            if (first != null)
            {
                return Mapper.Map<T>(first, _selectedFields);
            }

            return default;
        }

        public List<T> ToList()
        {
            RowSet results = Session.Execute(BuildQuery());
            List<T> mapped = new List<T>();

            foreach (Row row in results)
            {
                mapped.Add(Mapper.Map<T>(row, _selectedFields));
            }

            return mapped;
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            CurrentQuery.Limit = new IntegerConstant() { Value = 1 };
            RowSet results = await Session.ExecuteAsync(BuildQuery());
            Row first = results.FirstOrDefault();

            if (first != null)
            {
                return Mapper.Map<T>(first, _selectedFields);
            }

            return default;
        }

        public async Task<List<T>> ToListAsync()
        {
            RowSet results = await Session.ExecuteAsync(BuildQuery());
            List<T> mapped = new List<T>(results.Count());

            foreach (Row row in results)
            {
                mapped.Add(Mapper.Map<T>(row, _selectedFields));
            }

            return mapped;
        }

        private void AddOrder(Expression<Func<T, object>> column, Order.OrderTypes orderType)
        {
            Expression current = column?.Body;

            if (current == null)
            {
                return;
            }

            if (current is UnaryExpression unaryExpression &&
               (unaryExpression.NodeType == ExpressionType.Convert ||
                unaryExpression.NodeType == ExpressionType.ConvertChecked))
            {
                current = unaryExpression.Operand;
            }

            if (current is MemberExpression member)
            {
                (CurrentQuery.OrderingClause.Orders as List<Order>).Add(new Order()
                {
                    OrderType = orderType,
                    FieldName = member.Member.Name
                });
            }
        }
    }
}
