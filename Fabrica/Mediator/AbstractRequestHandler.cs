using System;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Exceptions;
using Fabrica.Rql.Parser;
using Fabrica.Rules.Exceptions;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;

namespace Fabrica.Mediator
{


    public abstract class AbstractRequestHandler<TRequest,TResponse>: IRequestHandler<TRequest,TResponse> where TRequest: class, IRequest<TResponse> where TResponse: FluentResponse<TResponse>, new()
    {


        protected AbstractRequestHandler()
        {
            Correation = new Correlation();
        }


        protected AbstractRequestHandler( ICorrelation correlation )
        {
            Correation = correlation;
        }


        protected ICorrelation Correation { get; }
        private ILogger _logger;
        protected ILogger GetLogger()
        {
            return _logger ??= Correation.GetLogger(this);
        }

        protected virtual TResponse CreateFailureResponse()
        {
            return new TResponse();
        }

        public async Task<TResponse> Handle( TRequest request, CancellationToken cancellationToken )
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.LogObject(nameof(request), request );



                // *****************************************************************
                logger.Debug("Attempting to call Before");
                await Before(request);



                // *****************************************************************
                logger.Debug("Attempting to call Perform");
                var response = await Perform(request, cancellationToken);



                // *****************************************************************
                logger.Debug("Attempting to call Success");
                response = await Success( request, response );



                // *****************************************************************
                return response;


            }
            catch ( NotFoundException cause )
            {
                logger.DebugFormat(cause, " Caught {0} in {1}", cause.GetType().FullName, GetType().FullName);
                return CreateFailureResponse().From( cause );
            }
            catch ( ViolationsExistException cause )
            {
                logger.DebugFormat(cause, " Caught {0} in {1}", cause.GetType().FullName, GetType().FullName);
                return CreateFailureResponse().From( cause );
            }
            catch ( RqlException cause )
            {
                logger.DebugFormat(cause, " Caught {0} in {1}", cause.GetType().FullName, GetType().FullName);
                return CreateFailureResponse().From(cause).WithKind(ErrorKind.BadRequest).WithErrorCode( "InvalidRQL" );
            }
            catch ( ExternalException cause )
            {
                logger.ErrorFormat( cause, " Caught {0} in {1}", cause.GetType().FullName, GetType().FullName );
                return CreateFailureResponse().From( cause );
            }
            catch (Exception cause)
            {
                logger.Error(cause, "Unhandled exception encountered");
                var ec = cause.GetType().FullName ?? "";
                return CreateFailureResponse().WithKind(ErrorKind.System).WithErrorCode(ec).WithExplaination(cause.Message);
            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        protected virtual Task Before( TRequest request )
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                return Task.CompletedTask;

            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        protected virtual Task<TResponse> Success( TRequest request, TResponse response )
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                return Task.FromResult(response);

            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        protected virtual void Failure( TRequest request, Exception cause )
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.ErrorWithContext( cause, request, "Handle failed" );

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        protected abstract Task<TResponse> Perform( TRequest request, CancellationToken cancellationToken );


    }


    public abstract class AbstractRequestHandler2<TRequest, TResponse> : IRequestHandler<TRequest, Response<TResponse>> where TRequest : class, IRequest<Response<TResponse>>
    {


        protected AbstractRequestHandler2()
        {
            Correation = new Correlation();
        }


        protected AbstractRequestHandler2(ICorrelation correlation)
        {
            Correation = correlation;
        }


        protected ICorrelation Correation { get; }
        private ILogger _logger;
        protected ILogger GetLogger()
        {
            return _logger ??= Correation.GetLogger(this);
        }

        protected virtual Response<TResponse> CreateFailureResponse()
        {
            return new Response<TResponse>();
        }

        public async Task<Response<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.LogObject(nameof(request), request);



                // *****************************************************************
                logger.Debug("Attempting to call Before");
                await Before(request);



                // *****************************************************************
                logger.Debug("Attempting to call Perform");
                var response = await Perform(request, cancellationToken);



                // *****************************************************************
                logger.Debug("Attempting to call Success");
                response = await Success(request, response);



                // *****************************************************************
                return new Response<TResponse>(response).IsOk();


            }
            catch (NotFoundException cause)
            {
                logger.DebugFormat(cause, " Caught {0} in {1}", cause.GetType().FullName, GetType().FullName);
                return CreateFailureResponse().From(cause);
            }
            catch (ViolationsExistException cause)
            {
                logger.DebugFormat(cause, " Caught {0} in {1}", cause.GetType().FullName, GetType().FullName);
                return CreateFailureResponse().From(cause);
            }
            catch (RqlException cause)
            {
                logger.DebugFormat(cause, " Caught {0} in {1}", cause.GetType().FullName, GetType().FullName);
                return CreateFailureResponse().From(cause).WithKind(ErrorKind.BadRequest).WithErrorCode("InvalidRQL");
            }
            catch (ExternalException cause)
            {
                logger.ErrorFormat(cause, " Caught {0} in {1}", cause.GetType().FullName, GetType().FullName);
                return CreateFailureResponse().From(cause);
            }
            catch (Exception cause)
            {
                logger.Error(cause, "Unhandled exception encountered");
                var ec = cause.GetType().FullName ?? "";
                return CreateFailureResponse().WithKind(ErrorKind.System).WithErrorCode(ec).WithExplaination(cause.Message);
            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        protected virtual Task Before(TRequest request)
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                return Task.CompletedTask;

            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        protected virtual Task<TResponse> Success(TRequest request, TResponse response)
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                return Task.FromResult(response);

            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        protected virtual void Failure(TRequest request, Exception cause)
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.ErrorWithContext(cause, request, "Handle failed");

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        protected abstract Task<TResponse> Perform(TRequest request, CancellationToken cancellationToken);


    }


    public abstract class AbstractRequestHandler2<TRequest> : IRequestHandler<TRequest, Response> where TRequest : class, IRequest<Response>
    {


        protected AbstractRequestHandler2()
        {
            Correation = new Correlation();
        }


        protected AbstractRequestHandler2(ICorrelation correlation)
        {
            Correation = correlation;
        }


        protected ICorrelation Correation { get; }
        private ILogger _logger;
        protected ILogger GetLogger()
        {
            return _logger ??= Correation.GetLogger(this);
        }

        protected virtual Response CreateFailureResponse()
        {
            return new Response();
        }

        public async Task<Response> Handle(TRequest request, CancellationToken cancellationToken)
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.LogObject(nameof(request), request);



                // *****************************************************************
                logger.Debug("Attempting to call Before");
                await Before(request);



                // *****************************************************************
                logger.Debug("Attempting to call Perform");
                await Perform(request, cancellationToken);



                // *****************************************************************
                logger.Debug("Attempting to call Success");
                await Success(request);



                // *****************************************************************
                return new Response().IsOk();


            }
            catch (NotFoundException cause)
            {
                logger.DebugFormat(cause, " Caught {0} in {1}", cause.GetType().FullName, GetType().FullName);
                return CreateFailureResponse().From(cause);
            }
            catch (ViolationsExistException cause)
            {
                logger.DebugFormat(cause, " Caught {0} in {1}", cause.GetType().FullName, GetType().FullName);
                return CreateFailureResponse().From(cause);
            }
            catch (RqlException cause)
            {
                logger.DebugFormat(cause, " Caught {0} in {1}", cause.GetType().FullName, GetType().FullName);
                return CreateFailureResponse().From(cause).WithKind(ErrorKind.BadRequest).WithErrorCode("InvalidRQL");
            }
            catch (ExternalException cause)
            {
                logger.ErrorFormat(cause, " Caught {0} in {1}", cause.GetType().FullName, GetType().FullName);
                return CreateFailureResponse().From(cause);
            }
            catch (Exception cause)
            {
                logger.Error(cause, "Unhandled exception encountered");
                var ec = cause.GetType().FullName ?? "";
                return CreateFailureResponse().WithKind(ErrorKind.System).WithErrorCode(ec).WithExplaination(cause.Message);
            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        protected virtual Task Before(TRequest request)
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                return Task.CompletedTask;

            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        protected virtual Task Success(TRequest request)
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                return Task.CompletedTask;

            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        protected virtual void Failure(TRequest request, Exception cause)
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.ErrorWithContext(cause, request, "Handle failed");

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        protected abstract Task Perform(TRequest request, CancellationToken cancellationToken);


    }





}
