using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisFreeOne
{
    public class RedisStore
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;

        static RedisStore()
        {
            var configurationOptions = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                ResponseTimeout = 100000,
                //EndPoints = { "testcache.5c5tua.0001.use1.cache.amazonaws.com:6379" }
                EndPoints = { "localhost:6379" }
            };

            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions));
        }

        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        public static IDatabase RedisCache => Connection.GetDatabase();
    }
}

