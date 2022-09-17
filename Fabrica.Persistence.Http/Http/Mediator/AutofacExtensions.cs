using Autofac;
using Fabrica.Persistence.Http.Mediator.Handlers;

namespace Fabrica.Persistence.Http.Mediator
{

    public static class AutofacExtensions
    {


        public static ContainerBuilder AddHttpClientMediatorHandlers(this ContainerBuilder builder)
        {


            builder.RegisterGeneric(typeof(HttpQueryHandler<>))
                .AsImplementedInterfaces()
                .InstancePerDependency();

            builder.RegisterGeneric(typeof(HttpRetrieveHandler<>))
                .AsImplementedInterfaces()
                .InstancePerDependency();

            builder.RegisterGeneric(typeof(HttpCreateHandler<>))
                .AsImplementedInterfaces()
                .InstancePerDependency();

            builder.RegisterGeneric(typeof(HttpCreateMemberHandler<,>))
                .AsImplementedInterfaces()
                .InstancePerDependency();


            builder.RegisterGeneric(typeof(HttpUpdateHandler<>))
                .AsImplementedInterfaces()
                .InstancePerDependency();

            builder.RegisterGeneric(typeof(HttpDeleteHandler<>))
                .AsImplementedInterfaces()
                .InstancePerDependency();

            builder.RegisterGeneric(typeof(HttpPatchHandler<>))
                .AsImplementedInterfaces()
                .InstancePerDependency();


            return builder;

        }


    }
}