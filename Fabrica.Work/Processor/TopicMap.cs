using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Fabrica.Exceptions;

namespace Fabrica.Work.Processor
{

    public class TopicMap
    {

        private ReaderWriterLockSlim Lock { get; } = new();

        private IReadOnlyDictionary<string,string> Map { get; set; }

        public void Load( IEnumerable<KeyValuePair<string, string>> pairs )
        {

            var dict = new Dictionary<string, string>(pairs);
            var map = new ReadOnlyDictionary<string, string>(dict);

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

        public string GetUri(string topic)
        {

            Lock.EnterReadLock();
            try
            {

                if( Map.TryGetValue(topic, out var uri) )
                    return uri;

                throw new PredicateException($"A URI for Topic ({topic}) could not be found");

            }
            finally
            {
                Lock.ExitReadLock();
            }


        }





    }

}
