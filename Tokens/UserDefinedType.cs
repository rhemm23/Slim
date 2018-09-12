using System.Text;

namespace CassandraORM.Tokens
{
    public class UserDefinedType : CqlType
    {
        public override Types Type => Types.UserDefined;
        public string KeyspaceName { get; set; }
        public string Identifier { get; set; }

        public override void WriteCql(StringBuilder queryBuilder)
        {
            if (!string.IsNullOrWhiteSpace(KeyspaceName))
            {
                queryBuilder.Append($"{KeyspaceName}.");
            }

            queryBuilder.Append(Identifier);
        }
    }
}
