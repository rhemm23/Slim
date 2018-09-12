using Cassandra;
using CassandraORM.Cache;
using CassandraORM.Tokens;
using System.Collections.Generic;

namespace CassandraORM.Builders
{
    public abstract class QueryBuilder<T, TItem> where T : IQuery, new()
    {
        protected ISession Session { get; }
        protected T CurrentQuery { get; set; }
        protected BuilderHelper<T, TItem> BuilderHelper { get; }
        public Dictionary<string, object> Parameters { get; protected set; }
        private int _aliasInteger = 0;

        public QueryBuilder()
        {
            Session = GlobalConfiguration.Current.Session;
            BuilderHelper = new BuilderHelper<T, TItem>(this, GlobalConfiguration.Current.UseBindings);
            Parameters = new Dictionary<string, object>();
            CurrentQuery = new T();
            CurrentQuery.TableName = TableCache<TItem>.CachedTable.TableName;
            CurrentQuery.KeyspaceName = TableCache<TItem>.CachedTable.KeyspaceName;
        }

        public string GetDistinctAlias()
        {
            return $"p{_aliasInteger++:00}";
        }

        public SimpleStatement BuildQuery()
        {
            string queryText = CurrentQuery.WriteQuery().ToString();

            if (Parameters.Count == 0)
            {
                return new SimpleStatement(queryText);
            }

            return new SimpleStatement(Parameters, queryText);
        }
    }
}
