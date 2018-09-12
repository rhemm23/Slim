using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CassandraORM.Tokens
{
    public class SelectClause : IToken
    {
        public IEnumerable<KeyValuePair<Select, string>> Selects { get; set; }

        public void WriteCql(StringBuilder queryBuilder)
        {
            if (Selects == null)
            {
                queryBuilder.Append("*");

                return;
            }

            int count = Selects.Count();

            if (count == 1)
            {
                KeyValuePair<Select, string> select = Selects.ElementAt(0);

                select.Key.WriteCql(queryBuilder);

                if (!string.IsNullOrWhiteSpace(select.Value))
                {
                    queryBuilder.Append($" AS {select.Value}");
                }
            }
            else if (count > 1)
            {
                queryBuilder.Append("(");

                bool first = true;

                foreach (KeyValuePair<Select, string> select in Selects)
                {
                    if (!first)
                    {
                        queryBuilder.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    select.Key.WriteCql(queryBuilder);

                    if (!string.IsNullOrWhiteSpace(select.Value))
                    {
                        queryBuilder.Append($" AS {select.Value}");
                    }
                }

                queryBuilder.Append(")");
            }
        }
    }
}
