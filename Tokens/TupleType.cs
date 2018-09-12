using System.Collections.Generic;
using System.Text;

namespace CassandraORM.Tokens
{
    public class TupleType : CqlType
    {
        public override Types Type => Types.Tuple;
        public IEnumerable<CqlType> TypeArguments { get; set; }

        public override void WriteCql(StringBuilder queryBuilder)
        {
            if (TypeArguments == null)
            {
                return;
            }

            bool first = true;

            queryBuilder.Append("TUPLE <");

            foreach (CqlType cqlType in TypeArguments)
            {
                if (!first)
                {
                    queryBuilder.Append(", ");
                }
                else
                {
                    first = false;
                }

                cqlType.WriteCql(queryBuilder);
            }

            queryBuilder.Append(">");
        }
    }
}
