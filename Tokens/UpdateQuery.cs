using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CassandraORM.Tokens
{
    public class UpdateQuery : IQuery
    {
        public ICollection<UpdateParameter> UpdateParameters { get; set; }
        public ICollection<Assignment> Assignments { get; set; }
        public ICollection<Condition> Conditions { get; set; }
        public WhereClause WhereClause { get; set; }
        public bool IfExists { get; set; }
        public string TableName { get; set; }
        public string KeyspaceName { get; set; }

        public UpdateQuery()
        {
            UpdateParameters = new List<UpdateParameter>();
            Assignments = new List<Assignment>();
            Conditions = new List<Condition>();
        }

        public StringBuilder WriteQuery()
        {
            StringBuilder queryBuilder = new StringBuilder($"UPDATE {KeyspaceName}.{TableName}");

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

            if (Assignments != null &&
                Assignments.Count > 0)
            {
                queryBuilder.AppendLine();
                queryBuilder.Append("SET ");

                bool first = true;

                foreach (Assignment assignment in Assignments)
                {
                    if (!first)
                    {
                        queryBuilder.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    assignment.WriteCql(queryBuilder);
                }
            }

            if (WhereClause != null)
            {
                queryBuilder.AppendLine();
                WhereClause.WriteCql(queryBuilder);
            }

            if (Conditions != null &&
                Conditions.Count > 0)
            {
                queryBuilder.AppendLine();
                queryBuilder.Append("IF ");

                bool first = false;

                foreach (Condition condition in Conditions)
                {
                    if (!first)
                    {
                        queryBuilder.Append(" AND ");
                    }
                    else
                    {
                        first = false;
                    }

                    condition.WriteCql(queryBuilder);
                }
            }
            else if (IfExists)
            {
                queryBuilder.AppendLine();
                queryBuilder.Append("IF EXISTS");
            }

            queryBuilder.Append(";");
            return queryBuilder;
        }
    }
}
