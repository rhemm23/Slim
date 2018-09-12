using System.Collections.Generic;
using System.Reflection;

namespace CassandraORM.Models
{
    internal class Table
    {
        public string KeyspaceName { get; set; }

        public string TableName { get; set; }

        public Dictionary<string, PropertyInfo> Columns { get; set; }

        public Dictionary<string, string> Aliases { get; set; }
    }
}
