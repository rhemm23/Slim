using System.Text;

namespace CassandraORM.Tokens
{
    public interface IQuery
    {
        string TableName { get; set; }
        string KeyspaceName { get; set; }
        StringBuilder WriteQuery();
    }
}
