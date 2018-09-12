using System.Text;

namespace CassandraORM.Tokens
{
    public abstract class Term : IToken
    {
        public abstract TermTypes Type { get; }

        public enum TermTypes
        {
            Constant,
            Literal,
            FunctionCall,
            ArithmeticOperation,
            BindMarker
        }

        public abstract void WriteCql(StringBuilder queryBuilder);
    }
}
