using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace Fabrica.Http
{


    public static class AutofacExtensions
    {


        public static ContainerBuilder AddHttpClient( this ContainerBuilder builder, string name, string baseUri="", int retryCount=5 )
        {


            var delay = Backoff.DecorrelatedJitterBackoffV2( TimeSpan.FromSeconds(1), retryCount, fastFirst: true );
            var retry = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync( delay );


            var services = new ServiceCollection();

            services.AddHttpClient(name, c =>
                {

                    if( !string.IsNullOrWhiteSpace(baseUri) )
                        c.BaseAddress = new Uri(baseUri);

                })
                .AddPolicyHandler(retry);


            builder.Populate(services);


            return builder;

        }

    }





}
