using System.Text;

namespace CassandraORM.Tokens
{
    public class BoolConstant : Constant<bool>
    {
        public override void WriteCql(StringBuilder queryBuilder)
        {
            queryBuilder.Append(Value ? "TRUE" : "FALSE");
        }
    }
}
