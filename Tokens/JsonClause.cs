using System.Text;

namespace CassandraORM.Tokens
{
    public class JsonClause : IToken, IInsertable
    {
        public IJsonValue Json { get; set; }
        public JsonDefaults Default { get; set; }

        public enum JsonDefaults
        {
            None,
            Null,
            Unset
        }

        public void WriteCql(StringBuilder queryBuilder)
        {
            queryBuilder.Append("JSON ");
            Json.WriteCql(queryBuilder);

            if (Default != JsonDefaults.None)
            {
                queryBuilder.Append(" DEFAULT ");

                switch (Default)
                {
                    case JsonDefaults.Null:
                        queryBuilder.Append("NULL");
                        break;

                    case JsonDefaults.Unset:
                        queryBuilder.Append("UNSET");
                        break;
                }
            }
        }
    }
}
