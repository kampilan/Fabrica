using Fabrica.Watch.Sink;
using Microsoft.Extensions.Logging;

namespace Fabrica.Watch.Bridges.MicrosoftImpl;

public class Logger: Microsoft.Extensions.Logging.ILogger
{

    public Logger( ILogger logger, string category )
    {
        InternalLogger = logger;
        Category       = category;
    }


    protected ILogger InternalLogger { get; private set; }
    private string Category { get; }

    private DateTime LastLevelCheck { get; set; } = DateTime.MinValue;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {


        if( !IsEnabled(logLevel) )
            return;

        var title = formatter(state, exception);

        var level = _map(logLevel);

        ILogEvent le;

        if (exception != null)
            le = InternalLogger.CreateEvent(level, title, exception, null! );
        else
            le = InternalLogger.CreateEvent(level, title);

        InternalLogger.LogEvent( le );

    }


    public bool IsEnabled( LogLevel logLevel )
    {

        if( (DateTime.Now - LastLevelCheck).TotalSeconds > 15 )
        {
            LastLevelCheck = DateTime.Now;
            InternalLogger = WatchFactoryLocator.Factory.GetLogger(Category);
        }


        switch (logLevel)
        {
            case LogLevel.Trace:
                return InternalLogger.IsTraceEnabled;
            case LogLevel.Debug:
                return InternalLogger.IsDebugEnabled;
            case LogLevel.Information:
                return InternalLogger.IsInfoEnabled;
            case LogLevel.Warning:
                return InternalLogger.IsWarningEnabled;
            case LogLevel.Error:
                return InternalLogger.IsErrorEnabled;
            case LogLevel.Critical:
                return InternalLogger.IsErrorEnabled;
        }

        return false;

    }


    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return new LoggingContext();
    }


    private Level _map(LogLevel level)
    {

        switch (level)
        {
            case LogLevel.Trace:
                return Level.Trace;
            case LogLevel.Debug:
                return Level.Debug;
            case LogLevel.Information:
                return Level.Info;
            case LogLevel.Warning:
                return Level.Warning;
            case LogLevel.Error:
                return Level.Error;
            case LogLevel.Critical:
                return Level.Error;
            case LogLevel.None:
                return Level.Quiet;
        }
            
        return Level.Quiet;

    }


}