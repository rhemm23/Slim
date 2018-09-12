using System.Collections.Generic;
using System.Text;

namespace CassandraORM.Tokens
{
    public class Select : IToken
    {
        private IEnumerable<Select> _functionArguments;
        private string _functionIdentifier;
        private string _columnName;
        private SelectTypes _type;
        private Term _term;
        private Select _castSelect;
        private CqlType _castType;

        public Select()
        {
            _type = SelectTypes.Count;
        }

        public Select(string columnName)
        {
            _type = SelectTypes.Column;
            _columnName = columnName;
        }

        /// <summary>
        /// Select a single term
        /// </summary>
        public Select(Term term)
        {
            _type = SelectTypes.Term;
            _term = term;
        }

        /// <summary>
        /// Convert a select to a cql type
        /// </summary>
        public Select(Select select, CqlType cqlType)
        {
            _type = SelectTypes.Cast;
            _castSelect = select;
            _castType = cqlType;
        }

        /// <summary>
        /// Select a function
        /// </summary>
        public Select(string functionIdentifier, IEnumerable<Select> functionArguments)
        {
            _type = SelectTypes.Function;
            _functionArguments = functionArguments;
            _functionIdentifier = functionIdentifier;
        }

        public enum SelectTypes
        {
            Count,
            Function,
            Cast,
            Term,
            Column
        }

        public void WriteCql(StringBuilder queryBuilder)
        {
            switch (_type)
            {
                case SelectTypes.Cast:
                    queryBuilder.Append("CAST (");
                    _castSelect.WriteCql(queryBuilder);
                    queryBuilder.Append(" AS ");
                    _castType.WriteCql(queryBuilder);
                    queryBuilder.Append(")");
                    break;

                case SelectTypes.Column:
                    queryBuilder.Append(_columnName);
                    break;

                case SelectTypes.Count:
                    queryBuilder.Append("COUNT (*)");
                    break;

                case SelectTypes.Function:
                    queryBuilder.Append($"{_functionIdentifier} (");

                    if (_functionArguments != null)
                    {
                        bool first = true;

                        foreach (Select select in _functionArguments)
                        {
                            if (!first)
                            {
                                queryBuilder.Append(", ");
                            }
                            else
                            {
                                first = false;
                            }

                            select.WriteCql(queryBuilder);
                        }
                    }

                    queryBuilder.Append(")");
                    break;

                case SelectTypes.Term:
                    _term.WriteCql(queryBuilder);
                    break;
            }
        }
    }
}
