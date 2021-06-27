using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Fabrica.Utilities.Container;
using Microsoft.Extensions.Configuration;

namespace Fabrica.Work.Topics
{

    

    public class TopicMap: CorrelatedObject, ITopicMap, IDisposable
    {


        public TopicMap( ICorrelation correlation ) : base( correlation )
        {

            Lock = new ReaderWriterLockSlim();

        }


        private ReaderWriterLockSlim Lock { get; }

        private IReadOnlyDictionary<string,ITopicEndpoint> Map { get; set; }


        public void Load( string endpointName, IConfigurationSection section )
        {

            using var logger = EnterMethod();

            var dict = section.Get<Dictionary<string, string>>();
            var eps  = new Dictionary<string, ITopicEndpoint>();
            foreach (var pair in dict)
                eps[pair.Key] = new TopicEndpoint { Topic = pair.Key, Name = endpointName, Path = pair.Value };

            Load( eps );

        }

        public void Load( IEnumerable<KeyValuePair<string, ITopicEndpoint>> pairs )
        {

            var dict = new Dictionary<string, ITopicEndpoint>(pairs);
            var map  = new ReadOnlyDictionary<string, ITopicEndpoint>(dict);

            Lock.EnterWriteLock();
            try
            {
                Map = map;
            }
            finally
            {
                Lock.ExitWriteLock();
            }


        }


        public bool HasTopic(string topic)
        {

            Lock.EnterReadLock();
            try
            {

                return Map.ContainsKey(topic);

            }
            finally
            {
                Lock.ExitReadLock();
            }

        }


        public bool TryGetEndpoint( string topic, out ITopicEndpoint endpoint )
        {

            using var logger = EnterMethod();

            logger.Inspect(nameof(topic), topic);


            Lock.EnterReadLock();
            try
            {

                if( Map.TryGetValue(topic, out var ep) )
                {
                    endpoint = ep;
                    return true;
                }
                else
                {
                    endpoint = null;
                    return false;
                }


            }
            finally
            {
                Lock.ExitReadLock();
            }

        }


        public void Dispose()
        {
            Lock?.Dispose();
        }

    }

}
