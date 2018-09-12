using System.Text;

namespace CassandraORM.Tokens
{
    public class BindMarker : Term, IRestrictedValue, IJsonValue
    {
        public override TermTypes Type => TermTypes.BindMarker;
        public string Identifier { get; set; }

        public override void WriteCql(StringBuilder queryBuilder)
        {
            if (string.IsNullOrWhiteSpace(Identifier))
            {
                queryBuilder.Append("?");
            }
            else
            {
                queryBuilder.Append($":{Identifier}");
            }
        }
    }
}
