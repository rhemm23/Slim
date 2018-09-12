using System.Text;

namespace CassandraORM.Tokens
{
    public class Order : IToken
    {
        public OrderTypes OrderType { get; set; }
        public string FieldName { get; set; }

        public enum OrderTypes
        {
            Ascending,
            Descending
        }

        public void WriteCql(StringBuilder queryBuilder)
        {
            string orderType = null;

            switch (OrderType)
            {
                case OrderTypes.Ascending:
                    orderType = "ASC";
                    break;

                case OrderTypes.Descending:
                    orderType = "DESC";
                    break;
            }

            queryBuilder.Append($"{FieldName} {orderType}");
        }
    }
}
