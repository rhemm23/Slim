using System.Linq;
using System.Text;

namespace CassandraORM.Tokens
{
    public class SelectQuery : IQuery
    {
        public WhereClause WhereClause { get; set; }
        public OrderingClause OrderingClause { get; set; }
        public GroupingClause GroupingClause { get; set; }
        public SelectClause SelectClause { get; set; }
        public string KeyspaceName { get; set; }
        public string TableName { get; set; }
        public bool Json { get; set; }
        public bool Distinct { get; set; }
        public bool AllowFiltering { get; set; }
        public IRestrictedValue PerPartitionLimit { get; set; }
        public IRestrictedValue Limit { get; set; }

        public StringBuilder WriteQuery()
        {
            StringBuilder queryBuilder = new StringBuilder("SELECT ");

            if (Json)
            {
                queryBuilder.Append("JSON ");
            }
            else if (Distinct)
            {
                queryBuilder.Append("DISTINCT ");
            }

            if (SelectClause == null)
            {
                queryBuilder.Append("*");
            }
            else
            {
                SelectClause.WriteCql(queryBuilder);
            }

            queryBuilder.AppendLine();
            queryBuilder.Append($"FROM ");

            if (KeyspaceName == null)
            {
                queryBuilder.Append(TableName);
            }
            else
            {
                queryBuilder.Append($"{KeyspaceName}.{TableName}");
            }

            if (WhereClause != null)
            {
                queryBuilder.AppendLine();
                WhereClause.WriteCql(queryBuilder);
            }

            if (GroupingClause != null &&
                GroupingClause.FieldNames != null &&
                GroupingClause.FieldNames.Count() > 0)
            {
                queryBuilder.AppendLine();
                GroupingClause.WriteCql(queryBuilder);
            }

            if(OrderingClause != null && 
               OrderingClause.Orders != null &&
               OrderingClause.Orders.Count() > 0)
            {
                queryBuilder.AppendLine();
                OrderingClause.WriteCql(queryBuilder);
            }

            if(PerPartitionLimit != null)
            {
                queryBuilder.AppendLine();
                queryBuilder.Append("PER PARTITION LIMIT ");
                PerPartitionLimit.WriteCql(queryBuilder);
            }

            if(Limit != null)
            {
                queryBuilder.AppendLine();
                queryBuilder.Append("LIMIT ");
                Limit.WriteCql(queryBuilder);
            }

            if(AllowFiltering)
            {
                queryBuilder.AppendLine();
                queryBuilder.Append("ALLOW FILTERING");
            }

            queryBuilder.Append(";");

            return queryBuilder;
        }
    }
}
