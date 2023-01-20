using Microsoft.Extensions.Logging;

namespace Fabrica.Watch.Bridges.MicrosoftImpl;

public class LoggerProvider: ILoggerProvider
{


    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {

        var internalLogger = WatchFactoryLocator.Factory.GetLogger(categoryName);
        var logger = new Logger( internalLogger, categoryName );
        return logger;

    }


    public void Dispose()
    {
    }


}