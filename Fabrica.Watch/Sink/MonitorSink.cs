using System.Collections.Concurrent;

namespace Fabrica.Watch.Sink;

public class MonitorSink: IEventSink
{

    public void Start()
    {
    }

    public void Stop()
    {
    }

    private ConcurrentQueue<ILogEvent> Events { get; } = new();

    public bool Accumulate { get; set; }

    public int Total => _total;
    public int Count => Events.Count; 
    public IEnumerable<ILogEvent> GetEvents() => Events;


    public void Flush()
    {
        Events.Clear();
    }


    private int _total;
    public Task Accept(ILogEvent logEvent)
    {


        if( Accumulate )
            Events.Enqueue(logEvent);

        Interlocked.Increment(ref _total);

        return Task.CompletedTask;

    }

    public Task Accept(IEnumerable<ILogEvent> batch)
    {

        foreach (var le in batch)
        {

            if( Accumulate )
                Events.Enqueue(le);

            Interlocked.Increment(ref _total);

        }

        return Task.CompletedTask;

    }

}