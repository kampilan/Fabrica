using Fabrica.Api.Support.Models;
using Fabrica.Exceptions;
using Fabrica.Rules.Exceptions;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;

namespace Fabrica.Api.Support.Middleware;

public class ExceptionMonitorMiddleware
{

    public ExceptionMonitorMiddleware(RequestDelegate next)
    {

        Next = next;

    }

    private RequestDelegate Next { get; }


    public async Task Invoke(HttpContext httpContext, ICorrelation correlation)
    {

        if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
        if (correlation == null) throw new ArgumentNullException(nameof(correlation));


        try
        {
            await Next(httpContext);
        }
        catch (Exception cause)
        {

            Correlation = correlation;


            var error = BuildResponseModel(cause);

            if ( httpContext.Response.HasStarted )
            {
                using var logger = correlation.GetLogger(GetType());
                logger.Debug( cause, "Caught unhandled Exception after Response started" );
                logger.LogObject(nameof(error), error);
                return;
            }


            httpContext.Response.StatusCode = MapExceptionToStatus(cause);
            httpContext.Response.ContentType = "application/json";

            using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await using var jwriter = new JsonTextWriter(writer);
            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            serializer.Serialize(jwriter, error);
            await jwriter.FlushAsync();

            stream.Seek(0, SeekOrigin.Begin);
            await stream.CopyToAsync(httpContext.Response.Body);
        }
    }



    protected ICorrelation Correlation { get; set; } = null!;


    protected virtual int MapExceptionToStatus(Exception exception)
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

        logger.DebugFormat("Mapping Exception ({0}) to StatusCode {1}", exception.GetType().FullName ?? "", statusCode);


        return (int)statusCode;


    }


    protected virtual ErrorResponseModel BuildResponseModel(Exception exception)
    {

        if (exception == null) throw new ArgumentNullException(nameof(exception));


        using var logger = Correlation.EnterMethod(GetType());


        // ***********************************************************************

        if( exception is JsonReaderException je )
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
        if( exception is ViolationsExistException ve )
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


        var diagLogger = Correlation.GetLogger("Fabrica.Diagnostics.Http");

        // ***********************************************************************
        if ( exception is ExternalException bex )
        {

            var errorRes = new ErrorResponseModel
            {
                CorrelationId = Correlation.Uid,
                ErrorCode = bex.ErrorCode,
                Explanation = bex.Explanation,
                Details = bex.Details
            };


            if (bex.Kind == ErrorKind.System)
                diagLogger.Error(exception, "HTTP Request: Encountered unhandled Exception");
            else
                logger.Debug(exception, "External Exception");


            return errorRes;

        }



        // ***********************************************************************
        var defErrorRes = new ErrorResponseModel
        {
            CorrelationId = Correlation.Uid,
            ErrorCode = "Internal",
            Explanation = "An unhandled exception was encountered. Examine logs for details"
        };


        diagLogger.Error(exception, "HTTP Request: Encountered unhandled Exception");


        return defErrorRes;


    }










}