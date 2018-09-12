using Cassandra;
using CassandraORM.Cache;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CassandraORM
{
    internal static class Mapper
    {
        public static T Map<T>(Row row, IEnumerable<string> fields = null)
        {
            T obj = Activator.CreateInstance<T>();

            if (fields != null)
            {
                foreach (string field in fields)
                {
                    PropertyInfo property;
                    if (TableCache<T>.CachedTable.Columns.TryGetValue(field, out property))
                    {
                        property.SetValue(obj, row[field]);
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<string, PropertyInfo> property in TableCache<T>.CachedTable.Columns)
                {
                    property.Value.SetValue(obj, row[property.Key]);
                }
            }

            return obj;
        }
    }
}
