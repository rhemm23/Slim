using System.Text;

namespace CassandraORM.Tokens
{
    public class Operator : IToken
    {
        public OperatorTypes OperatorType { get; }

        public enum OperatorTypes
        {
            Equal,
            Less,
            Greater,
            LessEqual,
            GreaterEqual,
            NotEqual,
            In,
            Contains,
            ContainsKey
        }

        public Operator(OperatorTypes operatorType)
        {
            OperatorType = operatorType;
        }

        public void WriteCql(StringBuilder queryBuilder)
        {
            switch (OperatorType)
            {
                case OperatorTypes.NotEqual:
                    queryBuilder.Append("!=");
                    break;

                case OperatorTypes.Equal:
                    queryBuilder.Append("=");
                    break;

                case OperatorTypes.In:
                    queryBuilder.Append("IN");
                    break;

                case OperatorTypes.Less:
                    queryBuilder.Append("<");
                    break;

                case OperatorTypes.Contains:
                    queryBuilder.Append("CONTAINS");
                    break;

                case OperatorTypes.ContainsKey:
                    queryBuilder.Append("CONTAINS KEY");
                    break;

                case OperatorTypes.LessEqual:
                    queryBuilder.Append("<=");
                    break;

                case OperatorTypes.Greater:
                    queryBuilder.Append(">");
                    break;

                case OperatorTypes.GreaterEqual:
                    queryBuilder.Append(">=");
                    break;
            }
        }
    }
}
