using System.Text;

namespace CassandraORM.Tokens
{
    public class FloatConstant : Constant<float>
    {
        public override void WriteCql(StringBuilder queryBuilder)
        {
            queryBuilder.Append(Value);
        }
    }
}
