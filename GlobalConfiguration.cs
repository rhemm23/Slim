namespace CassandraORM
{
    public static class GlobalConfiguration
    {
        internal static Configuration Current { get; private set; }

        public static void SetConfiguration(Configuration config)
        {
            Current = config;
        }
    }
}
