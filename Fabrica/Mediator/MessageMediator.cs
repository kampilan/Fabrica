using Autofac;
using Fabrica.Rules;
using Fabrica.Rules.Exceptions;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;

namespace Fabrica.Mediator;

public class MessageMediator : CorrelatedObject, IMessageMediator
{


    protected class WrapperServiceProvider : IServiceProvider
    {

        public WrapperServiceProvider(ILifetimeScope scope)
        {
            Scope = scope;
        }

        private ILifetimeScope Scope { get; }

        public object? GetService(Type serviceType)
        {
            return Scope.ResolveOptional(serviceType);
        }

    }



    public MessageMediator(ICorrelation correlation, IRuleSet rules, ILifetimeScope root ) : base(correlation)
    {
        Rules     = rules;
        RootScope = root;
    }

    private IRuleSet Rules { get; }
    private ILifetimeScope RootScope { get; }


    private void Evaluate(params object[] facts)
    {

        using var logger = EnterMethod();

        try
        {

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

    }


    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {

        ArgumentNullException.ThrowIfNull(request);

        using var logger = EnterMethod();


        logger.LogObject(nameof(request), request);


        // *****************************************************************
        logger.Debug("Attempting to evaluate request");
        Evaluate(request);



        // *****************************************************************
        logger.Debug("Attempting to build inner Mediator");
        var provider = new WrapperServiceProvider(RootScope);
        var mediator = new MediatR.Mediator(provider);




        // *****************************************************************
        logger.Debug("Attempting to send request through the mediator");
        var response = await mediator.Send(request, cancellationToken);

        logger.LogObject(nameof(response), response);



        // *****************************************************************
        return response;



    }


    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {

        ArgumentNullException.ThrowIfNull(request);

        using var logger = EnterMethod();


        logger.LogObject(nameof(request), request);



        // *****************************************************************
        logger.Debug("Attempting to evaluate request");
        Evaluate(request);



        // *****************************************************************
        logger.Debug("Attempting to build inner Mediator");
        var provider = new WrapperServiceProvider(RootScope);
        var mediator = new MediatR.Mediator(provider);



        // *****************************************************************
        logger.Debug("Attempting to send request through the mediator");
        await mediator.Send(request, cancellationToken);


    }


    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {


        ArgumentNullException.ThrowIfNull(notification);

        using var logger = EnterMethod();


        logger.LogObject(nameof(notification), notification);



        // *****************************************************************
        logger.Debug("Attempting to evaluate request");
        Evaluate(notification);



        // *****************************************************************
        logger.Debug("Attempting to resolve IMediator");
        var provider = new WrapperServiceProvider(RootScope);
        var mediator = new MediatR.Mediator(provider);



        // *****************************************************************
        logger.Debug("Attempting to send request through the mediator");
        await mediator.Publish(notification, cancellationToken);


    }


}

