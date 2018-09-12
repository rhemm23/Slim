using System.Collections.Generic;
using System.Text;

namespace CassandraORM.Tokens
{
    public class TupleLiteral : Literal
    {
        public override LiteralTypes LiteralType => LiteralTypes.Tuple;
        public IEnumerable<Term> Terms { get; set; }

        public override void WriteCql(StringBuilder queryBuilder)
        {
            if (Terms == null)
            {
                return;
            }

            bool first = true;

            queryBuilder.Append("(");

            foreach(Term term in Terms)
            {
                if (!first)
                {
                    queryBuilder.Append(", ");
                }
                else
                {
                    first = false;
                }

                term.WriteCql(queryBuilder);
            }

            queryBuilder.Append(")");
        }
    }
}
