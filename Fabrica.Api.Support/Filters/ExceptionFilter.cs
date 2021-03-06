using System;
using System.Net;
using System.Text;
using Fabrica.Api.Support.Models;
using Fabrica.Exceptions;
using Fabrica.Rules.Exceptions;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Fabrica.Watch.Sink;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace Fabrica.Api.Support.Filters
{


    public class ExceptionFilter : IExceptionFilter
    {


        public ExceptionFilter( ICorrelation correlation )
        {

            Correlation = correlation;

        }



        protected ICorrelation Correlation { get; }


        public virtual void OnException( ExceptionContext context )
        {

            if (context == null) throw new ArgumentNullException(nameof(context));


            using var logger = Correlation.EnterMethod(GetType());



            // *****************************************************************
            logger.Debug("Attempting to build response model");
            var response = BuildResponseModel(context.Exception);



            // *****************************************************************
            logger.Debug("Attempting to map exception to status code");
            var statusCode = MapExceptionToStatus(context.Exception);



            // *****************************************************************
            logger.Debug("Attempting to build result");
            context.Result = BuildResult(response, statusCode);


        }


        protected virtual int MapExceptionToStatus( [NotNull] Exception exception )
        {
            
            if (exception == null) throw new ArgumentNullException(nameof(exception));


            using var logger = Correlation.EnterMethod(GetType());



            var kind = ErrorKind.System;

            if (exception is ExternalException externalException)
                kind = externalException.Kind;
            else if (exception is JsonReaderException)
                kind = ErrorKind.BadRequest;


            var statusCode = HttpStatusCode.InternalServerError;

            switch (kind)
            {

                case ErrorKind.None:
                    statusCode = HttpStatusCode.OK;
                    break;

                case ErrorKind.NotFound:
                    statusCode = HttpStatusCode.UnprocessableEntity;
                    break;

                case ErrorKind.NotImplemented:
                    statusCode = HttpStatusCode.NotImplemented;
                    break;

                case ErrorKind.Predicate:
                    statusCode = HttpStatusCode.BadRequest;
                    break;

                case ErrorKind.Conflict:
                    statusCode = HttpStatusCode.Conflict;
                    break;

                case ErrorKind.Functional:
                    statusCode = HttpStatusCode.InternalServerError;
                    break;

                case ErrorKind.Concurrency:
                    statusCode = HttpStatusCode.Gone;
                    break;

                case ErrorKind.BadRequest:
                    statusCode = HttpStatusCode.BadRequest;
                    break;

                case ErrorKind.AuthenticationRequired:
                    statusCode = HttpStatusCode.Unauthorized;
                    break;

                case ErrorKind.NotAuthorized:
                    statusCode = HttpStatusCode.Forbidden;
                    break;

                case ErrorKind.System:
                case ErrorKind.Unknown:
                    statusCode = HttpStatusCode.InternalServerError;
                    break;

            }

            logger.DebugFormat("Mapping Exception ({0}) to StatusCode {1}", exception.GetType().FullName, statusCode);


            return (int)statusCode;


        }


        protected virtual ErrorResponseModel BuildResponseModel( [NotNull] Exception exception )
        {

            if (exception == null) throw new ArgumentNullException(nameof(exception));


            using var logger = Correlation.EnterMethod(GetType());



            // ***********************************************************************

            if (exception is JsonReaderException je)
            {
                var errorRes = new ErrorResponseModel
                {
                    CorrelationId = Correlation.Uid,
                    ErrorCode = "BadJsonRequest",
                    Explanation = $"Bad JSON in request near {je.Path} Line {je.LineNumber} Column {je.LinePosition}"
                };


                logger.Debug(exception, "JSON Exception");

                return errorRes;

            }



            // ***********************************************************************
            if (exception is ViolationsExistException ve)
            {
                var errorRes = new ErrorResponseModel
                {
                    CorrelationId = Correlation.Uid,
                    ErrorCode = "ValidationErrors",
                    Explanation = "Validation errors occurred",
                    Details = ve.Details
                };


                logger.Debug(exception, "Violations Exist");
                logger.LogObject("Violations", ve.Result.Events);


                return errorRes;

            }



            // ***********************************************************************
            if (exception is ExternalException bex)
            {

                var errorRes = new ErrorResponseModel
                {
                    CorrelationId = Correlation.Uid,
                    ErrorCode = bex.ErrorCode,
                    Explanation = bex.Explanation,
                    Details = bex.Details
                };


                if (bex.Kind == ErrorKind.System)
                    logger.Error(exception, "External Exception");
                else
                    logger.Debug(exception, "External Exception");


                return errorRes;

            }



            // ***********************************************************************
            var defErrorRes = new ErrorResponseModel
            {
                CorrelationId = Correlation.Uid,
                ErrorCode = "Internal",
                Explanation = "An unhandled exception was encountered."
            };


            logger.Error(exception, "Unhandled Exception");


            return defErrorRes;


        }


        protected virtual IActionResult BuildResult( [NotNull] ErrorResponseModel response, int statusCode )
        {
            
            if (response == null) throw new ArgumentNullException(nameof(response));


            using var logger = Correlation.EnterMethod(GetType());


            var result = new ObjectResult(response) { StatusCode = statusCode };

            var diagLogger = Correlation.GetLogger("Fabrica.Diagnostics.Http");

            logger.Inspect(nameof(diagLogger.IsDebugEnabled), diagLogger.IsDebugEnabled);


            if (diagLogger.IsDebugEnabled)
            {

                var builder = new StringBuilder();
                builder.AppendLine("********************************************************************************");
                builder.AppendLine();
                builder.AppendFormat("Error Code  : {0}", response.ErrorCode);
                builder.AppendLine();
                builder.AppendFormat("Explanation : {0}", response.Explanation);
                builder.AppendLine();
                builder.AppendFormat("Status Code : {0}", statusCode);
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine("********************************************************************************");


                var le = diagLogger.CreateEvent(Level.Debug, "HTTP Result", PayloadType.Text, builder.ToString());
                diagLogger.LogEvent(le);

            }


            return result;


        }


    }


}
