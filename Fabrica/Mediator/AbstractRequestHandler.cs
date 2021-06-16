using System;
using System.Runtime.CompilerServices;
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


    public abstract class AbstractRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, Response<TResponse>> where TRequest : class, IRequest<Response<TResponse>>
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

        protected TRequest Request { get; private set; }


        public async Task<Response<TResponse>> Handle(TRequest request, CancellationToken cancellationToken)
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.LogObject(nameof(request), request);


                Request = request;


                // *****************************************************************
                logger.Debug("Attempting to call Before");
                await Before(request);



                // *****************************************************************
                logger.Debug("Attempting to call Perform");
                var response = await Perform( cancellationToken );



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

            using var logger = GetLogger();

            return Task.CompletedTask;

        }

        protected virtual Task<TResponse> Success(TRequest request, TResponse response)
        {

            using var logger = GetLogger();

            return Task.FromResult(response);

        }

        protected virtual void Failure(TRequest request, Exception cause)
        {

            using var logger = GetLogger();

            logger.ErrorWithContext(cause, request, "Handle failed");

        }


        protected abstract Task<TResponse> Perform( CancellationToken cancellationToken=default );


    }


    public abstract class AbstractRequestHandler<TRequest> : IRequestHandler<TRequest, Response> where TRequest : class, IRequest<Response>
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


        protected TRequest Request { get; private set; }


        public async Task<Response> Handle(TRequest request, CancellationToken cancellationToken)
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

                logger.LogObject(nameof(request), request);


                Request = request;


                // *****************************************************************
                logger.Debug("Attempting to call Before");
                await Before(request);



                // *****************************************************************
                logger.Debug("Attempting to call Perform");
                await Perform( cancellationToken );



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

            using var logger = GetLogger();

            return Task.CompletedTask;

        }

        protected virtual Task Success(TRequest request)
        {

            using var logger = GetLogger();

            return Task.CompletedTask;

        }

        protected virtual void Failure(TRequest request, Exception cause)
        {

            using var logger = GetLogger();

            logger.ErrorWithContext(cause, request, "Handle failed");

        }


        protected abstract Task Perform( CancellationToken cancellationToken=default );


    }


}
