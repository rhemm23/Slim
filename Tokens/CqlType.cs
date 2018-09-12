using System.Text;

namespace CassandraORM.Tokens
{
    public abstract class CqlType : IToken
    {
        public abstract Types Type { get; }

        public enum Types
        {
            Tuple,
            Native,
            Collection,
            UserDefined
        }

        public abstract void WriteCql(StringBuilder queryBuilder);
    }
}
