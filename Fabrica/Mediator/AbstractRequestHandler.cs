
// ReSharper disable UnusedMember.Global

using System.Runtime.CompilerServices;
using Fabrica.Exceptions;
using Fabrica.Rql.Parser;
using Fabrica.Rules.Exceptions;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;

namespace Fabrica.Mediator;

public abstract class MediatorHandler
{
}

public abstract class AbstractRequestHandler<TRequest, TResponse> : MediatorHandler, IRequestHandler<TRequest, Response<TResponse>> where TRequest : class, IRequest<Response<TResponse>>
{


    protected AbstractRequestHandler()
    {
        Correlation = new Correlation();
    }


    protected AbstractRequestHandler(ICorrelation correlation)
    {
        Correlation = correlation;
    }


    protected ICorrelation Correlation { get; }

    protected ILogger GetLogger()
    {
        return Correlation.GetLogger(this);
    }

    protected ILogger EnterMethod( [CallerMemberName] string name = "" )
    {
        return Correlation.EnterMethod(GetType(), name);
    }


    protected virtual Response<TResponse> CreateFailureResponse()
    {
        return new Response<TResponse>();
    }

    protected TRequest Request { get; private set; } = null!;


    public async Task<Response<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
    {

         using var logger = EnterMethod();


        try
        {


            logger.LogObject(nameof(request), request);


            Request = request;


            // *****************************************************************
            logger.Debug("Attempting to call Before");
            await Before();



            // *****************************************************************
            logger.Debug("Attempting to call Perform");
            var response = await Perform( cancellationToken );



            // *****************************************************************
            logger.Debug("Attempting to call After");
            await After();



            // *****************************************************************
            logger.Debug("Attempting to call Success");
            response = await Success(request, response);


            // *****************************************************************
            logger.Debug("Attempting to call internal Success");
            await HandleSuccess();


            // *****************************************************************
            return new Response<TResponse>(response).IsOk();


        }
        catch (NotFoundException cause)
        {

            await HandleFailure();
            
            logger.Debug(cause, " Caught {0} in {1}", cause.GetType().FullName!, GetType().FullName!);
            return CreateFailureResponse().From(cause);

        }
        catch (ViolationsExistException cause)
        {

            await HandleFailure();

            logger.Debug(cause, " Caught {0} in {1}", cause.GetType().FullName!, GetType().FullName!);
            return CreateFailureResponse().From(cause);

        }
        catch (RqlException cause)
        {

            await HandleFailure();

            logger.Debug(cause, " Caught {0} in {1}", cause.GetType().FullName!, GetType().FullName!);
            return CreateFailureResponse().From(cause).WithKind(ErrorKind.BadRequest).WithErrorCode("InvalidRQL");

        }
        catch (ExternalException cause)
        {

            await HandleFailure();

            logger.Error(cause, " Caught {0} in {1}", cause.GetType().FullName!, GetType().FullName!);
            return CreateFailureResponse().From(cause);

        }
        catch (Exception cause)
        {

            await HandleFailure();

            logger.Error(cause, "Unhandled exception encountered");
            var ec = cause.GetType().FullName ?? "";
            return CreateFailureResponse().WithInner(cause).WithKind(ErrorKind.System).WithErrorCode(ec).WithExplanation(cause.Message);

        }


    }


    protected virtual Task Before()
    {

        return Task.CompletedTask;

    }


    protected virtual Task After()
    {

        return Task.CompletedTask;

    }


    protected virtual Task<TResponse> Success(TRequest request, TResponse response)
    {
        return Task.FromResult(response);
    }

    protected internal virtual Task HandleSuccess()
    {
        return Task.CompletedTask;
    }


    protected internal virtual Task HandleFailure()
    {
        return Task.CompletedTask;
    }


    protected abstract Task<TResponse> Perform( CancellationToken cancellationToken=default );


}


public abstract class AbstractRequestHandler<TRequest> : MediatorHandler, IRequestHandler<TRequest, Response>  where TRequest : class, IRequest<Response>
{


    protected AbstractRequestHandler()
    {
        Correlation = new Correlation();
    }


    protected AbstractRequestHandler(ICorrelation correlation)
    {
        Correlation = correlation;
    }


    protected ICorrelation Correlation { get; }

    protected ILogger GetLogger()
    {
        return Correlation.GetLogger(this);
    }

    protected ILogger EnterMethod( [CallerMemberName] string name = "" )
    {
        return Correlation.EnterMethod(GetType(), name);
    }




    protected virtual Response CreateFailureResponse()
    {
        return new Response();
    }


    protected TRequest Request { get; private set; } = null!;


    public async Task<Response> Handle(TRequest request, CancellationToken cancellationToken)
    {

         using var logger = EnterMethod();

        try
        {


            logger.LogObject(nameof(request), request);


            Request = request;


            // *****************************************************************
            logger.Debug("Attempting to call Before");
            await Before();



            // *****************************************************************
            logger.Debug("Attempting to call Perform");
            await Perform( cancellationToken );



            // *****************************************************************
            logger.Debug("Attempting to call After");
            await After();



            // *****************************************************************
            logger.Debug("Attempting to call Success");
            await Success(request);



            // *****************************************************************
            logger.Debug("Attempting to call internal Success");
            await HandleSuccess();



            // *****************************************************************
            return new Response().IsOk();


        }
        catch (NotFoundException cause)
        {

            await HandleFailure();

            logger.Debug(cause, " Caught {0} in {1}", cause.GetType().FullName!, GetType().FullName!);
            return CreateFailureResponse().From(cause);

        }
        catch (ViolationsExistException cause)
        {

            await HandleFailure();

            logger.Debug(cause, " Caught {0} in {1}", cause.GetType().FullName!, GetType().FullName!);
            return CreateFailureResponse().From(cause);

        }
        catch (RqlException cause)
        {

            await HandleFailure();

            logger.Debug(cause, " Caught {0} in {1}", cause.GetType().FullName!, GetType().FullName!);
            return CreateFailureResponse().From(cause).WithKind(ErrorKind.BadRequest).WithErrorCode("InvalidRQL");

        }
        catch (ExternalException cause)
        {

            await HandleFailure();

            logger.Error(cause, " Caught {0} in {1}", cause.GetType().FullName!, GetType().FullName! );
            return CreateFailureResponse().From(cause);

        }
        catch (Exception cause)
        {

            await HandleFailure();

            logger.Error(cause, "Unhandled exception encountered");
            var ec = cause.GetType().FullName ?? "";
            return CreateFailureResponse().WithKind(ErrorKind.System).WithErrorCode(ec).WithExplanation(cause.Message);

        }


    }


    protected virtual Task Before()
    {

        return Task.CompletedTask;

    }


    protected virtual Task After()
    {

        return Task.CompletedTask;

    }


    protected virtual Task Success(TRequest request)
    {
        return Task.CompletedTask;
    }

    protected virtual Task HandleSuccess()
    {
        return Task.CompletedTask;
    }


    protected virtual Task HandleFailure()
    {
        return Task.CompletedTask;
    }


    protected abstract Task Perform( CancellationToken cancellationToken=default );


}