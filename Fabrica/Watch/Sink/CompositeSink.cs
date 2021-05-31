using System.Collections.Generic;
using System.Linq;

namespace Fabrica.Watch.Sink
{
    public class CompositeSink: IEventSink
    {


        private IList<IEventSink> Sinks { get; } = new List<IEventSink>();

        public IEnumerable<IEventSink> InnerSinks => Sinks;


        public void AddSink(IEventSink sink)
        {
            Sinks.Add( sink );
        }


        public void Start()
        {

            foreach( var sink in Sinks )
                sink.Start();

        }

        public void Stop()
        {

            foreach (var sink in Sinks)
                sink.Stop();

        }


        public void Accept(ILogEvent logEvent)
        {

            foreach (var sink in Sinks)
                sink.Accept( logEvent );

        }

        public void Accept(IEnumerable<ILogEvent> batch)
        {


            if( Sinks.Count == 1 )
            {
                Sinks.First().Accept(batch);
                return;
            }


            if( Sinks.Count == 0 )
                return;


            var list = batch.ToList();
            foreach (var sink in Sinks)
                sink.Accept(list);

        }


    }

}
