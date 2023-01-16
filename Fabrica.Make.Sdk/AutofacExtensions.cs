using Autofac;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using System;
using System.Net.Http;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Fabrica.Make.Sdk
{

    public static class AutofacExtensions
    {

        public static ContainerBuilder AddMakeApiClient(this ContainerBuilder builder, string baseUri, string token)
        {

            var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 3, fastFirst: true);
            var retry = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(delay);


            var services = new ServiceCollection();

            services.AddHttpClient("MakeApi", c =>
                {
                    c.BaseAddress = string.IsNullOrWhiteSpace(baseUri) switch
                    {
                        false when baseUri.EndsWith("/") => new Uri(baseUri),
                        false => new Uri($"{baseUri}/"),
                        _ => c.BaseAddress
                    };
                })
                .AddHttpMessageHandler( ()=> new MakeRequestHandler(token) )
                .AddPolicyHandler(retry);


            builder.Populate(services);


            return builder;
        }

    }

}
