using System;
using System.Text;

namespace CassandraORM.Tokens
{
    public class GuidConstant : Constant<Guid>
    {
        public override void WriteCql(StringBuilder queryBuilder)
        {
            queryBuilder.Append(Value.ToString("D"));
        }
    }
}
