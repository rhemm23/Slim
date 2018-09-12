namespace CassandraORM.Tokens
{
    public abstract class Constant<T> : Term
    {
        public override TermTypes Type => TermTypes.Constant;
        public T Value { get; set; }
    }
}
