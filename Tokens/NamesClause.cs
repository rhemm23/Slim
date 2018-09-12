using System.Collections.Generic;
using System.Text;

namespace CassandraORM.Tokens
{
    public class NamesClause : IToken, IInsertable
    {
        public ICollection<string> ColumnNames { get; set; }
        public TupleLiteral Values { get; set; }

        public NamesClause()
        {
            ColumnNames = new List<string>();
        }

        public void WriteCql(StringBuilder queryBuilder)
        {
            if (ColumnNames != null &&
                ColumnNames.Count > 0)
            {
                queryBuilder.Append("(");

                bool first = true;

                foreach (string column in ColumnNames)
                {
                    if (!first)
                    {
                        queryBuilder.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    queryBuilder.Append(column);
                }

                queryBuilder.Append(")");
            }

            queryBuilder.Append(" VALUES ");

            Values.WriteCql(queryBuilder);
        }
    }
}
