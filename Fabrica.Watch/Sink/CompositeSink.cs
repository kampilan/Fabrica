using System.Transactions;

namespace Fabrica.Watch.Sink;

public class CompositeSink: IEventSink
{

    static CompositeSink()
    {

        ForObject = new NewtonsoftWatchObjectSerializer();
        ForException = new TextExceptionSerializer();

    }

    private static IWatchObjectSerializer ForObject { get; set; }
    private static IWatchExceptionSerializer ForException { get; set; }


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

        _enrich(logEvent);

        foreach (var sink in Sinks)
            await sink.Accept( logEvent );

        logEvent.Dispose();

    }

    public async Task Accept(IEnumerable<ILogEvent> batch)
    {

        if( Sinks.Count == 0 )
            return;

        var list = batch.ToList();
        list.ForEach(_enrich);

        foreach( var sink in Sinks )
            await sink.Accept(list);

        list.ForEach(e=>e.Dispose());
        list.Clear();

    }


    private void _enrich(ILogEvent logEvent)
    {

        if (logEvent.Error is not null)
        {
            var (type, source) = ForException.Serialize(logEvent.Error, logEvent.ErrorContext, logEvent.Retro);
            logEvent.Type = type;
            logEvent.Output = source;
        }
        else if (logEvent.Object is not null)
        {
            var (type, source) = ForObject.Serialize(logEvent.Object);
            logEvent.Type = type;
            logEvent.Output = source;
        }

        if (!string.IsNullOrWhiteSpace(logEvent.Output))
        {
            logEvent.Payload = WatchPayloadEncoder.Encode(logEvent.Output);
        }


    }


}