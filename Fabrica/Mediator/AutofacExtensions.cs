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
                builder.RegisterAssemblyTypes(assemblies)
                    .AsImplementedInterfaces()
                    .InstancePerDependency();
            }


            return builder;

        }


    }

}
