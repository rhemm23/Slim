using System.Text;

namespace CassandraORM.Tokens
{
    public class Condition : IToken
    {
        public SimpleSelection Selection { get; }
        public Operator Operator { get; }
        public Term Term { get; }

        public Condition(SimpleSelection selection, Operator operatorObj, Term term)
        {
            Selection = selection;
            Operator = operatorObj;
            Term = term;
        }

        public void WriteCql(StringBuilder queryBuilder)
        {
            Selection.WriteCql(queryBuilder);
            queryBuilder.Append(" ");
            Operator.WriteCql(queryBuilder);
            queryBuilder.Append(" ");
            Term.WriteCql(queryBuilder);
        }
    }
}
