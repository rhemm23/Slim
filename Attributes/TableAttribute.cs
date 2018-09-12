using System;

namespace CassandraORM.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string KeyspaceName { get; }
        public string TableName { get; }

        public TableAttribute(string keyspaceName, string tableName = null)
        {
            KeyspaceName = keyspaceName;
            TableName = tableName;
        }
    }
}
