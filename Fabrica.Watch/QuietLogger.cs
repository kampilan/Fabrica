using Fabrica.Watch.Sink;

namespace Fabrica.Watch;

public class QuietLogger: ILogger
{

    private static ILogEvent QuietEvent { get; } =  new LogEvent { Level = Level.Quiet };

    public void AddToRetro(string message)
    {
    }

    public ILogEvent CreateEvent( Level level, object? title )
    {
        return QuietEvent;
    }

    public ILogEvent CreateEvent( Level level, object? title, PayloadType type, string? payload )
    {
        return QuietEvent;
    }

    public ILogEvent CreateEvent( Level level, object? title, object? payload )
    {
        return QuietEvent;
    }

    public ILogEvent CreateEvent( Level level, object? title, Exception ex, object? context )
    {
        return QuietEvent;
    }

    public void LogEvent(ILogEvent logEvent)
    {
    }

    public string GetCurrentScope()
    {
        return "";
    }

    public void SetCurrentScope(string name)
    {
    }


    public IDisposable BeginScope<TState>(TState state)
    {
        return this;
    }


    public string Category => "";

    public bool IsTraceEnabled => false;
    public bool IsDebugEnabled => false;
    public bool IsInfoEnabled => false;
    public bool IsWarningEnabled => false;
    public bool IsErrorEnabled => false;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }


}