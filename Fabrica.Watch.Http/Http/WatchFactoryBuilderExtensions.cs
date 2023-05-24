using Fabrica.Watch.Http.Sink;

namespace Fabrica.Watch.Http;

public static class WatchFactoryBuilderExtensions
{


    public static RelayEventSink UseRelaySink(this WatchFactoryBuilder builder, int port = 5246, string? domain = null)
    {

        var sink = new RelayEventSink
        {
            Port = port
        };

        builder.Sinks.AddSink(sink);

        return sink;

    }


    public static HttpEventSink UseHttpSink(this WatchFactoryBuilder builder, string uri, string domain, bool useBatching = true, TimeSpan pollingInterval = default)
    {

        if (useBatching)
        {
            if (pollingInterval == default)
                pollingInterval = TimeSpan.FromMilliseconds(50);

            builder.UseBatching(20, pollingInterval);
        }

        var sink = new HttpEventSink
        {
            WatchEndpoint = uri,
            Domain = domain
        };

        builder.Sinks.AddSink(sink);

        return sink;

    }


}