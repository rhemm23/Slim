namespace CassandraORM.Tokens
{
    public abstract class Literal : Term
    {
        public override TermTypes Type => TermTypes.Literal;
        public abstract LiteralTypes LiteralType { get; }

        public enum LiteralTypes
        {
            Collection,
            UserDefinedType,
            Tuple
        }
    }
}
