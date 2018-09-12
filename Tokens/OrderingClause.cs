using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CassandraORM.Tokens
{
    public class OrderingClause : IToken
    {
        public IEnumerable<Order> Orders { get; set; }

        public void WriteCql(StringBuilder queryBuilder)
        {
            if (Orders == null)
            {
                return;
            }

            int count = Orders.Count();

            if (count > 0)
            {
                queryBuilder.Append("ORDER BY ");

                if (count == 1)
                {
                    Orders.ElementAt(0).WriteCql(queryBuilder);
                }
                else
                {
                    bool first = true;

                    queryBuilder.Append("(");

                    foreach (Order order in Orders)
                    {
                        if (!first)
                        {
                            queryBuilder.Append(", ");
                        }
                        else
                        {
                            first = false;
                        }

                        order.WriteCql(queryBuilder);
                    }

                    queryBuilder.Append(")");
                }
            }
        }
    }
}
