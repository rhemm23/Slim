using System.Text;

namespace CassandraORM.Tokens
{
    public class ArithmeticOperation : Term
    {
        public override TermTypes Type => TermTypes.ArithmeticOperation;
        public ArithmeticOperationTypes OperationType { get; set; }
        public Term FirstTerm { get; set; }
        public Term SecondTerm { get; set; }

        public enum ArithmeticOperationTypes
        {
            Negative,
            Addition,
            Subtraction,
            Multiplication,
            Division,
            Modulus
        }

        public override void WriteCql(StringBuilder queryBuilder)
        {
            if (FirstTerm == null)
            {
                return;
            }

            if (OperationType == ArithmeticOperationTypes.Negative)
            {
                queryBuilder.Append("-");

                FirstTerm.WriteCql(queryBuilder);
            }
            else
            {
                if (SecondTerm == null)
                {
                    return; 
                }

                queryBuilder.Append("(");

                FirstTerm.WriteCql(queryBuilder);

                switch (OperationType)
                {
                    case ArithmeticOperationTypes.Addition:
                        queryBuilder.Append(" + ");
                        break;

                    case ArithmeticOperationTypes.Subtraction:
                        queryBuilder.Append(" - ");
                        break;

                    case ArithmeticOperationTypes.Division:
                        queryBuilder.Append(" / ");
                        break;

                    case ArithmeticOperationTypes.Multiplication:
                        queryBuilder.Append(" * ");
                        break;

                    case ArithmeticOperationTypes.Modulus:
                        queryBuilder.Append(" % ");
                        break;
                }

                SecondTerm.WriteCql(queryBuilder);

                queryBuilder.Append(")");
            }
        }
    }
}
