using System.Text;

namespace CassandraORM.Tokens
{
    public class IntegerConstant : Constant<long>, IRestrictedValue
    {
        public override void WriteCql(StringBuilder queryBuilder)
        {
            queryBuilder.Append(Value);
        }
    }
}
