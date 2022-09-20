using System.Linq;
using System.Reflection;
using Autofac;
using Fabrica.Rules;
using Fabrica.Utilities.Container;

namespace Fabrica.Mediator
{

    public static class AutofacExtensions
    {

        public static ContainerBuilder UseMediator( this ContainerBuilder builder, params Assembly[] assemblies )
        {

            builder.Register(c =>
                {

                    var correlation = c.ResolveOptional<ICorrelation>() ?? new Correlation();
                    var rules       = c.Resolve<IRuleSet>();
                    var root        = c.Resolve<ILifetimeScope>();

                    var comp = new MessageMediator( correlation, rules, root );

                    return comp;

                })
                .As<IMessageMediator>()
                .InstancePerLifetimeScope();


            if( assemblies.Length > 0)
            {

                var types = assemblies.SelectMany(a => a.GetTypes()).Where(t => typeof(MediatorHandler).IsAssignableFrom(t));
               
                builder.RegisterTypes(types.ToArray())
                    .AsImplementedInterfaces()
                    .InstancePerDependency();

            }


            return builder;

        }


        public static ContainerBuilder AddHttpRpcHandler(this ContainerBuilder builder)
        {

            builder.RegisterGeneric(typeof(HttpRpcHandler<,>))
                .AsImplementedInterfaces()
                .InstancePerDependency();

            return builder;

        }


    }

}
