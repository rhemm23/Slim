using System.Collections.Generic;
using System.Reflection;

namespace CassandraORM.Models
{
    internal class UserDefinedType
    {
        public string KeyspaceName { get; set; }

        public string TypeName { get; set; }

        public Dictionary<string, PropertyInfo> Columns { get; set; }

        public Dictionary<string, string> Aliases { get; set; }
    }
}
