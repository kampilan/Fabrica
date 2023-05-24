using System.Net.Http.Json;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Watch.Sink;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;


// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Fabrica.Watch.Http.Sink
{

    public class HttpEventSink: IEventSink
    {

        public string WatchEndpoint { get; set; } = "";
        public string Domain { get; set; } = "";

        private IContainer Container { get; set; } = null!;
        private IHttpClientFactory Factory { get; set; } = null!;

        private ConsoleEventSink DebugSink { get; } = new();


        public void Start()
        {

            var builder = new ContainerBuilder();


            var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3, fastFirst: true);
            var retry = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(delay);


            var services = new ServiceCollection();

            services.AddHttpClient("", c =>
                {

                    if (!string.IsNullOrWhiteSpace(WatchEndpoint))
                        c.BaseAddress = new Uri(WatchEndpoint);

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

        public async Task Accept( ILogEvent logEvent )
        {

            var batch = new [] { logEvent };

            await Accept( batch );

        }

        public async Task Accept(IEnumerable<ILogEvent> batch)
        {

            try
            {

                using var client = Factory.CreateClient();

                var response = await client.PostAsJsonAsync( $"{Domain}", batch );
                response.EnsureSuccessStatusCode();


            }
            catch (Exception cause )
            {

                var le = new LogEvent
                {
                    Category = GetType().FullName??"",
                    Level = Level.Debug,
                    Title = cause.Message,
                    Payload = cause.StackTrace??""
                };

                await DebugSink.Accept(le);

            }


        }


    }

}
