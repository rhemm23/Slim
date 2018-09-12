using System.Text;

namespace CassandraORM.Tokens
{
    public interface IToken
    {
        void WriteCql(StringBuilder queryBuilder);
    }
}
