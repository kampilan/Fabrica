using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Rules.Exceptions;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using JetBrains.Annotations;
using MediatR;

namespace Fabrica.Mediator;

public class ScopedMessageMediator : CorrelatedObject, IMessageMediator
{

    public ScopedMessageMediator(ICorrelation correlation, IRuleSet rules, ILifetimeScope root) : base(correlation)
    {
        Rules = rules;
        RootScope = root;
    }

    private IRuleSet Rules { get; }

    private ILifetimeScope RootScope { get; }

    private void Evaluate(params object[] facts)
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


    public async Task<TResponse> Send<TResponse>([NotNull] IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {

        if (request == null) throw new ArgumentNullException(nameof(request));

        using var logger = EnterMethod();


        logger.LogObject(nameof(request), request);



        // *****************************************************************
        logger.Debug("Attempting to evaluate request");
        Evaluate(request);



        // *****************************************************************
        logger.Debug("Attempting to begin isolated scope");
        await using var scope = RootScope.BeginLifetimeScope();



        // *****************************************************************
        logger.Debug("Attempting to build inner Mediator");
        // ReSharper disable once AccessToDisposedClosure
        object Factory(Type type) => scope.Resolve(type);
        var mediator = new MediatR.Mediator(Factory);



        // *****************************************************************
        logger.Debug("Attempting to send request through the mediator");
        var response = await mediator.Send(request, cancellationToken);

        logger.LogObject(nameof(response), response);



        // *****************************************************************
        return response;



    }


    public async Task<object> Send(IRequest request, CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();


        logger.LogObject(nameof(request), request);



        // *****************************************************************
        logger.Debug("Attempting to evaluate request");
        Evaluate(request);



        // *****************************************************************
        logger.Debug("Attempting to begin isolated scope");
        await using var scope = RootScope.BeginLifetimeScope();



        // *****************************************************************
        logger.Debug("Attempting to build inner Mediator");
        // ReSharper disable once AccessToDisposedClosure
        object Factory(Type type) => scope.Resolve(type);
        var mediator = new MediatR.Mediator(Factory);



        // *****************************************************************
        logger.Debug("Attempting to send request through the mediator");
        var response = await mediator.Send(request, cancellationToken);

        logger.LogObject(nameof(response), response);



        // *****************************************************************
        return response;


    }


    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {


        using var logger = EnterMethod();


        logger.LogObject(nameof(notification), notification);



        // *****************************************************************
        logger.Debug("Attempting to evaluate request");
        Evaluate(notification);



        // *****************************************************************
        logger.Debug("Attempting to resolve IMediator");
        var mediator = RootScope.Resolve<IMediator>();



        // *****************************************************************
        logger.Debug("Attempting to send request through the mediator");
        await mediator.Publish(notification, cancellationToken);


    }


}