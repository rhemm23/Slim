using System.Collections.Generic;
using System.Text;

namespace CassandraORM.Tokens
{
    public class UserDefinedTypeLiteral : Literal
    {
        public override LiteralTypes LiteralType => LiteralTypes.UserDefinedType;
        public IDictionary<string, Term> Properties { get; set; }

        public override void WriteCql(StringBuilder queryBuilder)
        {
            if (Properties == null)
            {
                return;
            }

            queryBuilder.Append("{ ");

            bool first = true;

            foreach (KeyValuePair<string, Term> property in Properties)
            {
                if (!first)
                {
                    queryBuilder.Append(", ");
                }
                else
                {
                    first = false;
                }

                queryBuilder.Append($"{property.Key} : ");

                property.Value.WriteCql(queryBuilder);
            }

            queryBuilder.Append(" }");
        }
    }
}
