
// ReSharper disable UnusedMember.Global

using Fabrica.Exceptions;
using Fabrica.Models.Serialization;
using Fabrica.Rules.Exceptions;
using Fabrica.Utilities.Container;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Net;
using Fabrica.Watch;

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

            var serializer = new JsonSerializer
            {
                ContractResolver = new ModelContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };


            using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);
            await using var jwriter = new JsonTextWriter(writer);

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

        if (exception is JsonReaderException je)
        {
            var errorRes = new ProblemDetailModel
            {
                Type          = "BadJsonRequest",
                Title         = "Invalid JSON in Request",
                StatusCode    = statusCode,
                Detail        = $"Bad JSON in request near {je.Path} Line {je.LineNumber} Column {je.LinePosition}",
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

    [DefaultValue("")]
    [JsonProperty(nameof(Type))]
    public string Type{ get; set; } = "";

    [DefaultValue("")]
    [JsonProperty(nameof(Title))]
    public string Title { get; set; } = "";

    [DefaultValue(0)]
    [JsonProperty(nameof(StatusCode))]
    public int StatusCode { get; set; }

    [DefaultValue("")]
    [JsonProperty(nameof(Detail))]
    public string Detail { get; set; } = "";

    [DefaultValue("")]
    [JsonProperty(nameof(Instance))]
    public string Instance { get; set; } = "";

    [DefaultValue("")]
    [JsonProperty(nameof(CorrelationId))]
    public string CorrelationId { get; set; } = "";

    [JsonProperty(nameof(Segments))]
    [ExcludeEmpty]
    public IList<EventDetail> Segments { get; set; } = new List<EventDetail>();

}