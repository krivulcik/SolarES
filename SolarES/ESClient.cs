using Nest;
using System;
using System.Collections.Generic;

namespace SolarES
{
    public static class ESClient
    {
        public static string Server;

        private static readonly Dictionary<Type, string> _mappings = new Dictionary<Type, string>();

        public static void RegisterMapping(Type type, string name)
        {
            _mappings[type] = name;
        }

        public static ElasticClient Client { get { return GetClient.Value; } }

        private static readonly Lazy<ElasticClient> GetClient = new Lazy<ElasticClient>(() => {
            var local = new Uri(Server);

            ConnectionSettings settings = new ConnectionSettings(local)
                .DisableDirectStreaming();

            foreach (var mapping in _mappings)
            {
                settings.DefaultMappingFor(mapping.Key, m => m.IndexName(mapping.Value));
            }

            return new ElasticClient(settings);
        });
    }
}
