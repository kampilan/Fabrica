using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Utilities.Container;
using Microsoft.Extensions.DependencyInjection;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly;


// ReSharper disable UnusedMember.Global

namespace Fabrica.Services;

public static class AutofacExtensions
{


    public static ContainerBuilder AddServiceAddress(this ContainerBuilder builder, string name, string address)
    {

        builder.Register(_ => new ServiceAddress {ServiceName = name, Address = address})
            .AsSelf()
            .SingleInstance()
            .AutoActivate();

        return builder;

    }


    public static ContainerBuilder AddServiceClient(this ContainerBuilder builder, int retryCount = 3, TimeSpan timeout = default )
    {


        if( timeout == default )
            timeout = TimeSpan.FromSeconds(30);


        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), retryCount, fastFirst: true);
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay);

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);

        var policy = Policy.WrapAsync(retryPolicy, timeoutPolicy);



        var services = new ServiceCollection();

        services.AddHttpClient(ServiceClient.HttpClientName, _ =>
            {

            })
            .AddPolicyHandler(policy);


        builder.Populate(services);


        builder.Register(c =>
            {

                var comp = new ServiceEndpointResolver( Enumerable.Empty<ServiceEndpoint>() );

                return comp;

            })
            .AsSelf()
            .SingleInstance();


        builder.Register(c =>
            {

                var corr     = c.Resolve<ICorrelation>();
                var resolver = c.Resolve<ServiceEndpointResolver>();
                var factory  = c.Resolve<IHttpClientFactory>();

                var comp = new ServiceClient( corr, resolver, factory );

                return comp;

            })
            .AsSelf()
            .InstancePerLifetimeScope();


        return builder;

    }


    public static ContainerBuilder AddServiceClient(this ContainerBuilder builder, Func<IEnumerable<ServiceAddress>, IEnumerable<ServiceEndpoint>> binder, int retryCount = 3, TimeSpan timeout = default)
    {


        if (timeout == default)
            timeout = TimeSpan.FromSeconds(30);


        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), retryCount, fastFirst: true);
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay);

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(timeout);

        var policy = Policy.WrapAsync(retryPolicy, timeoutPolicy);



        var services = new ServiceCollection();

        services.AddHttpClient(ServiceClient.HttpClientName, _ =>
            {

            })
            .AddPolicyHandler(policy);


        builder.Populate(services);



        builder.Register(c =>
            {

                var addresses = c.Resolve<IEnumerable<ServiceAddress>>();
                var endpoints = binder(addresses);

                var comp = new ServiceEndpointResolver(endpoints);

                return comp;

            })
            .AsSelf()
            .SingleInstance();


        builder.Register(c =>
            {

                var corr     = c.Resolve<ICorrelation>();
                var resolver = c.Resolve<ServiceEndpointResolver>();
                var factory  = c.Resolve<IHttpClientFactory>();

                var comp = new ServiceClient(corr, resolver, factory);

                return comp;

            })
            .AsSelf()
            .InstancePerLifetimeScope();


        return builder;

    }



}