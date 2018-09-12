using System.Text;

namespace CassandraORM.Tokens
{
    public class StringConstant : Constant<string>, IJsonValue
    {
        public override void WriteCql(StringBuilder queryBuilder)
        {
            queryBuilder.Append($"'{Value.Replace("'", "''")}'");
        }
    }
}
