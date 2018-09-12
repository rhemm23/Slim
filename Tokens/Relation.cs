using System.Collections.Generic;
using System.Text;

namespace CassandraORM.Tokens
{
    public class Relation : IToken
    {
        private RelationTypes _relationType;
        public Operator Operator { get; }
        public IEnumerable<string> FieldNames { get; }
        public TupleLiteral TupleLiteral { get; }
        public string FieldName { get; }
        public Term Term { get; }

        private enum RelationTypes
        {
            SingleColumn,
            MultiColumnAndTuple,
            MultiColumnAndToken
        }

        public Relation(string fieldName, Operator operatorObj, Term term)
        {
            FieldName = fieldName;
            Operator = operatorObj;
            Term = term;
            _relationType = RelationTypes.SingleColumn;
        }

        public Relation(IEnumerable<string> fieldNames, Operator operatorObj, TupleLiteral tupleLiteral)
        {
            FieldNames = fieldNames;
            TupleLiteral = tupleLiteral;
            Operator = operatorObj;
            _relationType = RelationTypes.MultiColumnAndTuple;
        }

        public Relation(IEnumerable<string> tokenFieldNames, Operator operatorObj, Term term)
        {
            FieldNames = tokenFieldNames;
            Operator = operatorObj;
            Term = term;
            _relationType = RelationTypes.MultiColumnAndToken;
        }

        public void WriteCql(StringBuilder queryBuilder)
        {
            switch (_relationType)
            {
                case RelationTypes.SingleColumn:
                    queryBuilder.Append($"{FieldName} ");
                    Operator.WriteCql(queryBuilder);
                    queryBuilder.Append(" ");
                    Term.WriteCql(queryBuilder);
                    break;

                case RelationTypes.MultiColumnAndTuple:
                    WriteMultipleFields(queryBuilder);
                    queryBuilder.Append(" ");
                    Operator.WriteCql(queryBuilder);
                    queryBuilder.Append(" ");
                    TupleLiteral.WriteCql(queryBuilder);
                    break;

                case RelationTypes.MultiColumnAndToken:
                    queryBuilder.Append("TOKEN ");
                    WriteMultipleFields(queryBuilder);
                    queryBuilder.Append(" ");
                    Operator.WriteCql(queryBuilder);
                    queryBuilder.Append(" ");
                    Term.WriteCql(queryBuilder);
                    break;
            }    
        }

        private void WriteMultipleFields(StringBuilder queryBuilder)
        {
            queryBuilder.Append("(");

            if (FieldNames != null)
            {
                bool first = true;

                foreach (string fieldName in FieldNames)
                {
                    if (!first)
                    {
                        queryBuilder.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    queryBuilder.Append(fieldName);
                }
            }

            queryBuilder.Append(")");
        }
    }
}
