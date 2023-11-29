using Autofac;
using Fabrica.Utilities.Container;


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


    public static ContainerBuilder AddServiceClient(this ContainerBuilder builder)
    {

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

                var comp = new ServiceClient( corr, resolver );

                return comp;

            })
            .AsSelf()
            .InstancePerLifetimeScope();


        return builder;

    }


    public static ContainerBuilder AddServiceClient(this ContainerBuilder builder, Func<IEnumerable<ServiceAddress>, IEnumerable<ServiceEndpoint>> binder)
    {

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

                var corr = c.Resolve<ICorrelation>();
                var resolver = c.Resolve<ServiceEndpointResolver>();

                var comp = new ServiceClient(corr, resolver);

                return comp;

            })
            .AsSelf()
            .InstancePerLifetimeScope();


        return builder;

    }



}