using System.Text;

namespace CassandraORM.Tokens
{
    public class NullConstant : Constant<object>
    {
        public override void WriteCql(StringBuilder queryBuilder)
        {
            queryBuilder.Append("NULL");
        }
    }
}
