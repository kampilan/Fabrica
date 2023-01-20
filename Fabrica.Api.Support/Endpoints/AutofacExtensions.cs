
// ReSharper disable UnusedMember.Global

using Autofac;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Patch;
using Fabrica.Utilities.Container;

namespace Fabrica.Api.Support.Endpoints;

public static class AutofacExtensions
{

    private class EndpointComponentImpl : IEndpointComponent
    {

        public ICorrelation Correlation { get; init; } = null!;
        public IModelMetaService Meta { get; init; } = null!;
        public IMessageMediator Mediator { get; init; } = null!;
        public IPatchResolver Resolver { get; init; } = null!;

    }


    public static ContainerBuilder AddEndpointComponent(this ContainerBuilder builder)
    {


        builder.Register(c =>
            {

                var corr     = c.Resolve<ICorrelation>();
                var meta     = c.Resolve<IModelMetaService>();
                var mediator = c.Resolve<IMessageMediator>();
                var resolver = c.Resolve<IPatchResolver>();

                var comp = new EndpointComponentImpl
                {
                    Correlation = corr,
                    Meta        = meta,
                    Mediator    = mediator,
                    Resolver    = resolver
                };

                return comp;

            })
            .As<IEndpointComponent>()
            .InstancePerDependency();

        
        return builder;


    }

}