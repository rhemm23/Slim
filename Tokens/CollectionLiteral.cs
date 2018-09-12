using System.Collections.Generic;
using System.Text;

namespace CassandraORM.Tokens
{
    public class CollectionLiteral : Literal
    {
        public override LiteralTypes LiteralType => LiteralTypes.Collection;
        private IDictionary<Term, Term> _map;
        private IList<Term> _list;
        private ISet<Term> _set;
        
        public CollectionLiteral(IList<Term> elements)
        {
            _list = elements;
        }

        public CollectionLiteral(ISet<Term> elements)
        {
            _set = elements;
        }

        public CollectionLiteral(IDictionary<Term, Term> elements)
        {
            _map = elements;
        }

        public override void WriteCql(StringBuilder queryBuilder)
        {
            if (_map != null)
            {
                queryBuilder.Append("{ ");

                bool first = true;

                foreach(KeyValuePair<Term, Term> property in _map)
                {
                    if (!first)
                    {
                        queryBuilder.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    property.Key.WriteCql(queryBuilder);

                    queryBuilder.Append(" : ");

                    property.Value.WriteCql(queryBuilder);
                }

                queryBuilder.Append(" }");
            }
            else if (_set != null)
            {
                queryBuilder.Append("{ ");

                bool first = true;

                foreach (Term term in _set)
                {
                    if (!first)
                    {
                        queryBuilder.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    term.WriteCql(queryBuilder);
                }

                queryBuilder.Append(" }");
            }
            else if (_list != null)
            {
                queryBuilder.Append("[ ");

                bool first = true;

                foreach (Term term in _list)
                {
                    if (!first)
                    {
                        queryBuilder.Append(", ");
                    }
                    else
                    {
                        first = false;
                    }

                    term.WriteCql(queryBuilder);
                }

                queryBuilder.Append(" ]");
            }
        }
    }
}
