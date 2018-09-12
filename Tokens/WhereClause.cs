using System.Collections.Generic;
using System.Text;

namespace CassandraORM.Tokens
{
    public class WhereClause : IToken
    {
        public ICollection<Relation> Relations { get; set; }

        public WhereClause()
        {
            Relations = new List<Relation>();
        }

        public void WriteCql(StringBuilder queryBuilder)
        {
            if (Relations == null || 
                Relations.Count == 0)
            {
                return;
            }

            queryBuilder.Append("WHERE ");

            bool first = true;
            
            foreach (Relation relation in Relations)
            {
                if (!first)
                {
                    queryBuilder.Append(" AND ");
                }
                else
                {
                    first = false;
                }

                relation.WriteCql(queryBuilder);
            }
        }
    }
}
