using System.Collections.Generic;
using System.Text;

namespace CassandraORM.Tokens
{
    public class InsertQuery : IQuery
    {
        public ICollection<UpdateParameter> UpdateParameters { get; set; }
        public IInsertable InsertedData { get; set; }
        public string TableName { get; set; }
        public string KeyspaceName { get; set; }
        public bool IfNotExists { get; set; }

        public InsertQuery()
        {
            UpdateParameters = new List<UpdateParameter>();
        }

        public StringBuilder WriteQuery()
        {
            StringBuilder queryBuilder = new StringBuilder($"INSERT INTO {KeyspaceName}.{TableName} ");
            InsertedData.WriteCql(queryBuilder);

            if (IfNotExists)
            {
                queryBuilder.AppendLine();
                queryBuilder.Append("IF NOT EXISTS");
            }

            if (UpdateParameters != null &&
                UpdateParameters.Count > 0)
            {
                queryBuilder.AppendLine();
                queryBuilder.Append("USING ");

                bool first = true;

                foreach (UpdateParameter parameter in UpdateParameters)
                {
                    if (!first)
                    {
                        queryBuilder.Append(" AND ");
                    }
                    else
                    {
                        first = false;
                    }

                    parameter.WriteCql(queryBuilder);
                }
            }

            queryBuilder.Append(";");
            return queryBuilder;
        }
    }
}
