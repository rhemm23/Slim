using System;

namespace CassandraORM.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TypeAttribute : Attribute
    {
        public string KeyspaceName { get; }
        public string TypeName { get; }

        public TypeAttribute(string keyspaceName, string typeName = null)
        {
            KeyspaceName = keyspaceName;
            TypeName = typeName;
        }
    }
}
