using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CassandraORM.Tokens
{
    public class GroupingClause : IToken
    {
        public IEnumerable<string> FieldNames { get; set; }

        public void WriteCql(StringBuilder queryBuilder)
        {
            if (FieldNames == null)
            {
                return;
            }

            int count = FieldNames.Count();
            
            if (count > 0)
            {
                queryBuilder.Append("GROUP BY ");

                if (count == 1)
                {
                    queryBuilder.Append(FieldNames.ElementAt(0));
                }
                else
                {
                    queryBuilder.Append("(");

                    bool first = true;

                    foreach (string fieldName in FieldNames)
                    {
                        if (!first)
                        {
                            queryBuilder.Append(", ");
                        }
                        else
                        {
                            first = false;
                        }

                        queryBuilder.Append(fieldName);
                    }

                    queryBuilder.Append(")");
                }
            }
        }
    }
}
