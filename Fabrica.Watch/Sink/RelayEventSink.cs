using System.Net.Http.Json;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace Fabrica.Watch.Sink;

public class RelayEventSink : IEventSink
{

    public int Port { get; set; } = 5246;

    private IContainer Container { get; set; }
    private IHttpClientFactory Factory { get; set; }

    private ConsoleEventSink DebugSink { get; } = new();


    public void Start()
    {

        var builder = new ContainerBuilder();


        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3, fastFirst: true);
        var retry = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay);


        var services = new ServiceCollection();

        services.AddHttpClient("Fabrica.Watch.Relay", c =>
            {
                c.BaseAddress = new Uri($"http://localhost:{Port}/");
            })
            .AddPolicyHandler(retry);


        builder.Populate(services);

        Container = builder.Build();

        Factory = Container.Resolve<IHttpClientFactory>();

    }

    public void Stop()
    {
        Container.Dispose();
    }

    public async Task Accept(ILogEvent logEvent)
    {

        var batch = new[] { logEvent };

        await Accept(batch);

    }

    public async Task Accept(IEnumerable<ILogEvent> batch)
    {

        try
        {

            using var client = Factory.CreateClient("Fabrica.Watch.Relay");

            var response = await client.PostAsJsonAsync( "", batch );
            response.EnsureSuccessStatusCode();


        }
        catch (Exception cause)
        {

            var le = new LogEvent
            {
                Category = GetType().FullName,
                Level = Level.Debug,
                Title = cause.Message,
                Payload = cause.StackTrace
            };

            await DebugSink.Accept(le);

        }


    }

}