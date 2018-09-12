using Cassandra;

namespace CassandraORM
{
    public class Configuration
    {
        public ISession Session { get; set; }

        public Cluster Cluster { get; set; }

        public bool UseBindings { get; set; }
    }
}
