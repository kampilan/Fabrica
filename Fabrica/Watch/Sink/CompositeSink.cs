using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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


        public async Task Accept(ILogEvent logEvent)
        {

            foreach (var sink in Sinks)
                await sink.Accept( logEvent );

        }

        public async Task Accept(IEnumerable<ILogEvent> batch)
        {


            if( Sinks.Count == 1 )
            {
                await Sinks.First().Accept(batch);
                return;
            }


            if( Sinks.Count == 0 )
                return;


            var list = batch.ToList();
            foreach (var sink in Sinks)
                await sink.Accept(list);

        }


    }

}
