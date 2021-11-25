using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Rules;
using Fabrica.Rules.Exceptions;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;


// ReSharper disable AccessToDisposedClosure

namespace Fabrica.Mediator
{
   
    
    public class ScopedMessageMediator: IMessageMediator
    {


        public ScopedMessageMediator( ICorrelation correlation, IRuleSet rules, ILifetimeScope root)
        {
            Correlation = correlation;
            Rules       = rules;
            RootScope   = root;
        }
        
        private ICorrelation Correlation { get; }
        private IRuleSet Rules { get; }

        private ILifetimeScope RootScope { get; }

        private ILogger GetLogger() => Correlation.GetLogger(this);

        private void BuildScope( ContainerBuilder builder )
        {

            builder.Register(c =>
                {
                    var comp = new Correlation
                    {
                        Caller = Correlation.Caller,
                        Debug  = Correlation.Debug,
                        Tenant = Correlation.Tenant
                    };

                    return comp;

                })
                .As<ICorrelation>()
                .InstancePerLifetimeScope();

        }


        private void Evaluatate(params object[] facts)
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


                var ec = Rules.GetEvaluationContext();
                ec.ThrowNoRulesException = false;

                ec.AddAllFacts(facts);

                var result = Rules.Evaluate(ec);

                logger.LogObject(nameof(result), result);


            }
            catch (ViolationsExistException cause)
            {
                throw new MediatorInvalidRequestException(cause.Result.Events);
            }
            finally
            {
                logger.LeaveMethod();
            }

        }


        public async Task<TResponse> Send<TResponse>( IRequest<TResponse> request, CancellationToken cancellationToken = default )
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.LogObject(nameof(request), request);



                // *****************************************************************
                logger.Debug("Attempting to evaluate request");
                Evaluatate(request);



                // *****************************************************************
                logger.Debug("Attempting to create lifetime scope for request");
                await using( var scope = RootScope.BeginLifetimeScope(BuildScope) )
                {


                    // *****************************************************************
                    logger.Debug("Attempting to build inner Mediator");
                    object Factory(Type type) => scope.Resolve(type);
                    var mediator = new MediatR.Mediator(Factory);



                    // *****************************************************************
                    logger.Debug("Attempting to send request through the mediator");
                    var response = await mediator.Send(request, cancellationToken);

                    logger.LogObject(nameof(response), response);



                    // *****************************************************************
                    logger.Debug("Attempting to evaluate request/response pair");
                    Evaluatate(request,response);



                    // *****************************************************************
                    return response;

                }


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public async Task Publish<TNotification>( TNotification notification, CancellationToken cancellationToken = default ) where TNotification : INotification
        {


            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.LogObject(nameof(notification), notification);



                // *****************************************************************
                logger.Debug("Attempting to evaluate request");
                Evaluatate(notification);



                // *****************************************************************
                logger.Debug("Attempting to create lifetime scope for request");
                await using( var scope = RootScope.BeginLifetimeScope(BuildScope) )
                {


                    // *****************************************************************
                    logger.Debug("Attempting to resolve IMediator");
                    var mediator = scope.Resolve<IMediator>();



                    // *****************************************************************
                    logger.Debug("Attempting to send request through the mediator");
                    await mediator.Publish(notification, cancellationToken);


                }


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }


}
