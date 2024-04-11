using Microsoft.Extensions.Logging;

namespace Fabrica.Watch.Bridges.MicrosoftImpl;

public static class LoggingBuilderExtensions
{

    public static ILoggingBuilder UseWatch(this ILoggingBuilder builder)
    {

        builder.ClearProviders();
        builder.AddProvider(new LoggerProvider());
        builder.SetMinimumLevel(LogLevel.Trace);

        return builder;

    }

}