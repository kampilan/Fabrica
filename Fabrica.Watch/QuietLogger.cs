using Fabrica.Watch.Sink;

namespace Fabrica.Watch;

public class QuietLogger: ILogger
{

    public static ILogger Singleton { get; } = new QuietLogger();
        

    private static ILogEvent QuietEvent { get; } =  new LogEvent { Level = Level.Quiet };
 
    public ILogEvent CreateEvent( Level level, object title )
    {
        return QuietEvent;
    }

    public ILogEvent CreateEvent( Level level, object title, PayloadType type, string payload )
    {
        return QuietEvent;
    }

    public ILogEvent CreateEvent( Level level, object title, object payload )
    {
        return QuietEvent;
    }

    public ILogEvent CreateEvent( Level level, object title, Exception ex, object context )
    {
        return QuietEvent;
    }

    public void LogEvent(ILogEvent logEvent)
    {
    }

    public void Trace(object message)
    {
    }

    public void Trace(Func<string> expression)
    {
    }

    public void Trace(Exception ex, object message = null)
    {
    }

    public void TraceFormat(string template, params object[] args)
    {
    }

    public void TraceFormat(Exception ex, string template, params object[] args)
    {
    }

    public void Debug(object message)
    {
    }

    public void Debug(Func<string> expression)
    {
    }

    public void Debug(Exception ex, object message = null)
    {
    }

    public void DebugFormat(string template, params object[] args)
    {
    }

    public void DebugFormat(Exception ex, string template, params object[] args)
    {
    }

    public void Info(object message)
    {
    }

    public void Info(Func<string> expression)
    {
    }

    public void Info(Exception ex, object message = null)
    {
    }

    public void InfoFormat(string template, params object[] args)
    {
    }

    public void InfoFormat(Exception ex, string template, params object[] args)
    {
    }

    public void Warning(object message)
    {
    }

    public void Warning(Func<string> expression)
    {
    }

    public void Warning(Exception ex, object message = null)
    {
    }

    public void WarningWithContext(Exception ex, object context, object message = null)
    {
    }

    public void WarningFormat(string template, params object[] args)
    {
    }

    public void WarningFormat(Exception ex, string template, params object[] args)
    {
    }

    public void Error(object message)
    {
    }

    public void Error(Func<string> expression)
    {
    }

    public void Error(Exception ex, object message = null)
    {
    }

    public void ErrorWithContext(Exception ex, object context, object message = null)
    {
    }

    public void ErrorFormat(string template, params object[] args)
    {
    }

    public void ErrorFormat(Exception ex, string template, params object[] args)
    {
    }

    public void EnterMethod(string methodName = "")
    {
    }

    public void LeaveMethod(string methodName = "")
    {
    }

    public void EnterScope(string name)
    {
    }

    public void LeaveScope(string name)
    {
    }

    public void Inspect(object name, object value)
    {
    }

    public void LogSql(string title, string sql)
    {
    }

    public void LogXml(string title, string xml, bool pretty = true)
    {
    }

    public void LogJson(string title, string json, bool pretty = true)
    {
    }

    public void LogYaml(string title, string yaml)
    {
    }

    public void LogObject(string title, object source)
    {
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }


    public bool IsTraceEnabled => false;
    public bool IsDebugEnabled => false;
    public bool IsInfoEnabled => false;
    public bool IsWarningEnabled => false;
    public bool IsErrorEnabled => false;

    public void Dispose()
    {
    }

}