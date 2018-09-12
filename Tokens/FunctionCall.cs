using System.Collections.Generic;
using System.Text;

namespace CassandraORM.Tokens
{
    public class FunctionCall : Term
    {
        public override TermTypes Type => TermTypes.FunctionCall;
        public ICollection<Term> Arguments { get; set; }
        public string Identifier { get; set; }

        public FunctionCall()
        {
            Arguments = new List<Term>();
        }

        public override void WriteCql(StringBuilder queryBuilder)
        {
            queryBuilder.Append($"{Identifier} (");

            if (Arguments != null)
            {
                bool first = true;

                foreach (Term argument in Arguments)
                {
                    if (!first)
                    {
                        queryBuilder.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    argument.WriteCql(queryBuilder);
                }
            }

            queryBuilder.Append(")");
        }
    }
}
