using System.Text;

namespace CassandraORM.Tokens
{
    public class CollectionType : CqlType
    {
        public override Types Type => Types.Collection;
        public CollectionTypes CollectionTypeValue { get; set; }
        public CqlType TypeArgument { get; set; }

        public enum CollectionTypes
        {
            Map,
            Set,
            List
        }

        public override void WriteCql(StringBuilder queryBuilder)
        {
            switch (CollectionTypeValue)
            {
                case CollectionTypes.List:
                    queryBuilder.Append("LIST <");
                    break;

                case CollectionTypes.Map:
                    queryBuilder.Append("MAP <");
                    break;

                case CollectionTypes.Set:
                    queryBuilder.Append("SET <");
                    break;
            }

            TypeArgument.WriteCql(queryBuilder);

            queryBuilder.Append(">");
        }
    }
}
