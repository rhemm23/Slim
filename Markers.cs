using CassandraORM.Tokens;

namespace CassandraORM
{
    public static class Markers
    {
        /// <summary>
        /// Allows the parser to construct an alias for
        /// a member access expression
        /// </summary>
        public static T As<T>(this T current, string alias)
        {
            return default;
        }

        public static T Cast<T>(this T current, CqlType type)
        {
            return default;
        }

        public static bool Contains<T>(this T current, object value)
        {
            return default;
        }

        public static bool ContainsKey<T>(this T current, object key)
        {
            return default;
        }

        public static bool In<T>(this T current, params object[] values)
        {
            return default;
        }
    }
}
