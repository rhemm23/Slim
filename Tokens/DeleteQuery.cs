using System.Collections.Generic;
using System.Text;

namespace CassandraORM.Tokens
{
    public class DeleteQuery : IQuery
    {
        public ICollection<UpdateParameter> UpdateParameters { get; set; }
        public ICollection<SimpleSelection> Selections { get; set; }
        public ICollection<Condition> Conditions { get; set; }
        public WhereClause WhereClause { get; set; }
        public string TableName { get; set; }
        public string KeyspaceName { get; set; }
        public bool IfExists { get; set; }

        public DeleteQuery()
        {
            UpdateParameters = new List<UpdateParameter>();
            Selections = new List<SimpleSelection>();
            Conditions = new List<Condition>();
        }

        public StringBuilder WriteQuery()
        {
            StringBuilder queryBuilder = new StringBuilder("DELETE ");

            if (Selections != null &&
               Selections.Count > 0)
            {
                bool first = true;

                foreach (SimpleSelection selection in Selections)
                {
                    if (!first)
                    {
                        queryBuilder.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    selection.WriteCql(queryBuilder);
                }
            }

            queryBuilder.AppendLine();
            queryBuilder.Append($"FROM {KeyspaceName}.{TableName}");

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

                bool first = true;

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
