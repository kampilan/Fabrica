using System.Transactions;

namespace Fabrica.Watch.Sink;

public class CompositeSink: IEventSink
{

    private IList<IEventSink> Sinks { get; } = new List<IEventSink>();

    public IEnumerable<IEventSink> InnerSinks => Sinks;


    public void AddSink(IEventSink sink)
    {

        if( !_started )
            Sinks.Add( sink );

    }

    private bool _started;
    public void Start()
    {

        if (_started)
            return;

        foreach( var sink in Sinks )
            sink.Start();

        _started = true;

    }

    public void Stop()
    {

        foreach (var sink in Sinks)
            sink.Stop();

        Sinks.Clear();

    }


    public async Task Accept(ILogEvent logEvent)
    {

        WatchFactoryLocator.Factory.Encode(logEvent);

        foreach (var sink in Sinks)
            await sink.Accept( logEvent );

        logEvent.Dispose();

    }

    public async Task Accept(IEnumerable<ILogEvent> batch)
    {

        if( Sinks.Count == 0 )
            return;

        var list = batch.ToList();
        list.ForEach(WatchFactoryLocator.Factory.Encode);

        foreach( var sink in Sinks )
            await sink.Accept(list);

        list.ForEach(e=>e.Dispose());
        list.Clear();

    }



}