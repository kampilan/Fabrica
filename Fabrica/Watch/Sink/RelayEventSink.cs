using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Http;

namespace Fabrica.Watch.Sink
{

    public class RelayEventSink : IEventSink
    {

        public int Port { get; set; } = 5246;

        private IContainer Container { get; set; }
        private IHttpClientFactory Factory { get; set; }

        private ConsoleEventSink DebugSink { get; } = new();


        public void Start()
        {

            var builder = new ContainerBuilder();

            builder.AddHttpClient( "Fabrica.Watch.Relay", $"http://localhost:{Port}/");

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

}
