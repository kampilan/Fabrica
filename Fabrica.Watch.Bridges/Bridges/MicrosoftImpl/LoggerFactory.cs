
// ReSharper disable UnusedMember.Global

using Microsoft.Extensions.Logging;

namespace Fabrica.Watch.Bridges.MicrosoftImpl;

public class LoggerFactory: ILoggerFactory
{

    private LoggerProvider Provider { get; } = new LoggerProvider();


    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        var logger = Provider.CreateLogger(categoryName);
        return logger;
    }

    public void AddProvider( ILoggerProvider provider )
    {

    }

    public void Dispose()
    {
    }

}