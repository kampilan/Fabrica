using Autofac;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using MediatR;

namespace Fabrica.Mediator
{

    public static class AutofacExtensions
    {


        public static ContainerBuilder UseScopedMediator( this ContainerBuilder builder )
        {

            builder.Register(c =>
                {

                    var correlation = c.ResolveOptional<ICorrelation>() ?? new Correlation();
                    var rules       = c.Resolve<IRuleSet>();
                    var root        = c.Resolve<ILifetimeScope>();

                    var comp = new ScopedMessageMediator(correlation, rules, root);

                    return comp;

                })
                .As<IMessageMediator>()
                .InstancePerLifetimeScope();


            return builder;

        }

        public static ContainerBuilder UseMediator( this ContainerBuilder builder )
        {

            builder.Register(c =>
                {

                    var correlation = c.ResolveOptional<ICorrelation>() ?? new Correlation();
                    var rules       = c.Resolve<IRuleSet>();
                    var root        = c.Resolve<ILifetimeScope>();

                    var comp = new MessageMediator(correlation, rules, root);

                    return comp;

                })
                .As<IMessageMediator>()
                .InstancePerLifetimeScope();


            return builder;

        }



        public static ContainerBuilder AddMediatorMessageHandler<THandler,TRequest,TResponse>( this ContainerBuilder builder ) where THandler : class, IRequestHandler<TRequest,TResponse> where TRequest : class, IRequest<TResponse>
        {

            builder.RegisterType<THandler>()
                .As<IRequestHandler<TRequest, TResponse>>()
                .InstancePerDependency();

            return builder;
        }


    }

}
