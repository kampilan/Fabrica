using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Http;


// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Fabrica.Watch.Sink
{

    public class HttpEventSink: IEventSink
    {

        private class Model
        {

            public string Tag { get; set; } = "";
            public string Tenant { get; set; } = "";
            public string Subject { get; set; } = "";

            public string CorrelationId { get; set; } = "";

            public string Category { get; set; } = "";

            public string Level { get; set; } = "";

            public string Title { get; set; } = "";

            public string PayloadType { get; set; } = "";

            public string Payload { get; set; } = "";

        }


        public string WatchEndpoint { get; set; } = "";
        public string Domain { get; set; } = "";

        private IContainer Container { get; set; }
        private IHttpClientFactory Factory { get; set; }

        private ConsoleEventSink DebugSink { get; } = new();


        public void Start()
        {

            var builder = new ContainerBuilder();

            builder.AddHttpClient( "", WatchEndpoint );

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

            var models = batch.Select(le => new Model
                {
                    Category      = le.Category,
                    CorrelationId = le.CorrelationId,
                    Level         = le.Level.ToString(),
                    Subject       = le.Subject,
                    Tenant        = le.Tenant,
                    Tag           = le.Tag,
                    Title         = le.Title,
                    PayloadType   = le.Type.ToString(),
                    Payload       = le.Payload
                })
                .ToList();


            try
            {

                using var client = Factory.CreateClient();

                var response = await client.PostAsJsonAsync( $"{Domain}", models );
                response.EnsureSuccessStatusCode();


            }
            catch (Exception cause )
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
