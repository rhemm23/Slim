using CassandraORM.Attributes;
using CassandraORM.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CassandraORM.Cache
{
    internal static class UserDefinedTypeCache<T>
    {
        public static readonly UserDefinedType CachedType;

        static UserDefinedTypeCache()
        {
            TypeAttribute typeAttribute = typeof(T).GetCustomAttribute(typeof(TypeAttribute)) as TypeAttribute;

            if (typeAttribute == null)
            {
                throw new TypeLoadException($"Unable to cache type {typeof(T).Name} as it does not have a 'Type' attribute");
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

            CachedType = new UserDefinedType()
            {
                Columns = properties,
                KeyspaceName = typeAttribute.KeyspaceName,
                TypeName = typeAttribute.TypeName ?? typeof(T).Name,
                Aliases = aliases
            };
        }
    }
}
