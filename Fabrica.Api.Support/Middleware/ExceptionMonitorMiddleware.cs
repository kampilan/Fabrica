
// ReSharper disable UnusedMember.Global

using Fabrica.Exceptions;
using Fabrica.Models.Serialization;
using Fabrica.Rules.Exceptions;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Fabrica.Watch;

namespace Fabrica.Api.Support.Middleware;


public class ExceptionMonitorMiddleware(RequestDelegate next)
{
    
    private RequestDelegate Next { get; } = next;


    public async Task Invoke(HttpContext httpContext, ICorrelation correlation, JsonSerializerOptions options )
    {

        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(correlation);

        try
        {

            await Next(httpContext);

        }
        catch (Exception cause)
        {

            Correlation = correlation;


            if (httpContext.Response.HasStarted)
                return;


            var instance = $"{httpContext.Request.Path}";
            var statusCode = MapExceptionToStatus(cause);

            var error = BuildResponseModel(instance, statusCode, cause);


            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";


            using var stream = new MemoryStream();

            await JsonSerializer.SerializeAsync( stream, error, options );

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
        else if (exception is JsonException)
            kind = ErrorKind.BadRequest;


        var statusCode = HttpStatusCode.InternalServerError;

        switch (kind)
        {

            case ErrorKind.None:
                statusCode = HttpStatusCode.OK;
                break;

            case ErrorKind.NotFound:
                statusCode = HttpStatusCode.NotFound;
                break;

            case ErrorKind.NotImplemented:
                statusCode = HttpStatusCode.NotImplemented;
                break;

            case ErrorKind.Predicate:
                statusCode = HttpStatusCode.UnprocessableEntity;
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

        logger.Debug("Mapping Exception ({0}) to StatusCode {1}", exception.GetType().FullName ?? "", statusCode);


        return (int)statusCode;


    }


    protected virtual ProblemDetailModel BuildResponseModel(string instance, int statusCode,Exception exception)
    {

        if (exception == null) throw new ArgumentNullException(nameof(exception));


        using var logger = Correlation.EnterMethod(GetType());


        // ***********************************************************************

        if (exception is JsonException je)
        {
            var errorRes = new ProblemDetailModel
            {
                Type          = "BadJsonRequest",
                Title         = "Invalid JSON in Request",
                StatusCode    = statusCode,
                Detail        = $"Bad JSON in request near {je.Path} Line {je.LineNumber} Column {je.BytePositionInLine}",
                Instance      = instance,
                CorrelationId = Correlation.Uid,
            };


            logger.Debug(exception, "JSON Exception");

            return errorRes;

        }



        // ***********************************************************************
        if (exception is ViolationsExistException ve)
        {

            var errorRes = new ProblemDetailModel
            {
                Type          = "ValidationError",
                Title         = "Validation errors occurred",
                StatusCode    = statusCode,
                Detail        = $"Validation resulted in {ve.Details.Count} error(s).",
                Instance      = instance,
                CorrelationId = Correlation.Uid,
                Segments      = ve.Details
            };


            logger.Debug(exception, "Violations Exist");
            logger.LogObject("Violations", ve.Result.Events);


            return errorRes;

        }


        // ***********************************************************************
        if (exception is NotFoundException nfe)
        {

            var errorRes = new ProblemDetailModel
            {
                Type = nfe.ErrorCode,
                Title      = "Resource not found",
                StatusCode = statusCode,
                Detail     = nfe.Explanation,
                Instance   = instance,
            };

            logger.Debug(exception, "Resource not found");

            return errorRes;

        }



        var diagLogger = Correlation.GetLogger("Fabrica.Diagnostics.Http");

        // ***********************************************************************
        if (exception is ExternalException bex)
        {

            var errorRes = new ProblemDetailModel
            {
                Type          = bex.ErrorCode,
                Title         = "Error encountered",
                StatusCode    = statusCode,
                Detail        = bex.Explanation,
                Instance      = instance,
                CorrelationId = Correlation.Uid,
                Segments      = bex.Details
            };


            if (bex.Kind == ErrorKind.System)
                diagLogger.Error(exception, "HTTP Request: Encountered unhandled Exception");
            else
                logger.Debug(exception, "External Exception");


            return errorRes;

        }


        var defErrorRes = new ProblemDetailModel
        {
            Type          = "InternalError",
            Title         = "Internal Error encountered",
            StatusCode    = statusCode,
            Detail        = "An unhandled exception was encountered. Examine logs for details",
            Instance      = instance,
            CorrelationId = Correlation.Uid
        };


        diagLogger.Error(exception, "HTTP Request: Encountered unhandled Exception");


        return defErrorRes;


    }


}

public class ProblemDetailModel
{

    [JsonPropertyName(nameof(Type))]
    public string Type{ get; set; } = "";

    [JsonPropertyName(nameof(Title))]
    public string Title { get; set; } = "";

    [JsonPropertyName(nameof(StatusCode))]
    public int StatusCode { get; set; }

    [JsonPropertyName(nameof(Detail))]
    public string Detail { get; set; } = "";

    [JsonPropertyName(nameof(Instance))]
    public string Instance { get; set; } = "";

    [JsonPropertyName(nameof(CorrelationId))]
    public string CorrelationId { get; set; } = "";

    [JsonPropertyName(nameof(Segments))]
    [ExcludeEmpty]
    public IList<EventDetail> Segments { get; set; } = new List<EventDetail>();

}