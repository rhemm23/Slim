using CassandraORM.Attributes;
using CassandraORM.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CassandraORM.Cache
{
    internal static class TableCache<T>
    {
        public static readonly Table CachedTable;

        static TableCache()
        {
            TableAttribute tableAttribute = typeof(T).GetCustomAttribute(typeof(TableAttribute)) as TableAttribute;

            if (tableAttribute == null)
            {
                throw new TypeLoadException($"Unable to cache type {typeof(T).Name} as it does not have a 'Table' attribute");
            }

            Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
            Dictionary<string, string> aliases = new Dictionary<string, string>();

            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (Attribute.IsDefined(property, typeof(IgnoreAttribute)))
                {
                    continue;
                }

                ColumnAttribute column = property.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                string columnName = column?.Name ?? property.Name;

                properties.Add(columnName, property);
                aliases.Add(property.Name, columnName);
            }

            CachedTable = new Table()
            {
                Columns = properties,
                KeyspaceName = tableAttribute.KeyspaceName,
                TableName = tableAttribute.TableName ?? typeof(T).Name,
                Aliases = aliases
            };
        }
    }
}
