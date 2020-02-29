﻿using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BcGov.Fams3.Redis
{
    public class RedisConnectionFactory
    {
        private static Lazy<ConnectionMultiplexer> Connection;

        private static readonly string REDIS_CONNECTIONSTRING = "localhost";

        static RedisConnectionFactory()
        {
        }

        public static ConnectionMultiplexer OpenConnection(string redisConnectionStr)
        {
            if(Connection == null)
            {
                var options = ConfigurationOptions.Parse(redisConnectionStr);
                Connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));
            }
            return Connection.Value;           
        }
    }
}
