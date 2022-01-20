using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using Fabrica.Api.Support.Models;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Patch;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Support.Endpoints;

public abstract class BaseEndpoint: ControllerBase
{

    protected BaseEndpoint( IEndpointComponent component )
    {

        Component = component;

    }

    private IEndpointComponent Component { get; }

    protected ICorrelation Correlation => Component.Correlation;
    protected IModelMetaService Meta => Component.Meta;
    protected IMessageMediator Mediator => Component.Mediator;
    protected IPatchResolver Resolver => Component.Resolver;

    protected ILogger GetLogger()
    {

        var logger = Correlation.GetLogger(this);

        return logger;

    }

    protected ILogger EnterMethod([CallerMemberName] string name = "")
    {

        var logger = Correlation.EnterMethod(GetType(), name);

        return logger;

    }


    protected virtual HttpStatusCode MapErrorToStatus(ErrorKind kind)
    {

        using var logger = EnterMethod();

        logger.Inspect(nameof(kind), kind);


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

        logger.Inspect(nameof(statusCode), statusCode);


        return statusCode;

    }

    protected IActionResult BuildErrorResult( IExceptionInfo error )
    {


        using var logger = EnterMethod();


        logger.LogObject(nameof(error), error);



        // *****************************************************************
        logger.Debug("Attempting to build ErrorResponseModel");
        var model = new ErrorResponseModel
        {
            ErrorCode = error.ErrorCode,
            Explanation = error.Explanation,
            Details = new List<EventDetail>(error.Details),
            CorrelationId = Correlation.Uid
        };



        // *****************************************************************
        logger.Debug("Attempting to map error Kind to HttpStatusCode");
        var status = MapErrorToStatus(error.Kind);

        logger.Inspect(nameof(status), status);



        // *****************************************************************
        logger.Debug("Attempting to build ObjectResult");
        var result = new ObjectResult(model)
        {
            StatusCode = (int)status
        };



        // *****************************************************************
        return result;


    }


    protected virtual IActionResult BuildResult<TValue>( Response<TValue> response )
    {

        using var logger = EnterMethod();


        logger.LogObject(nameof(response), response);



        // *****************************************************************
        logger.Debug("Attempting to check for success");
        logger.Inspect(nameof(response.Ok), response.Ok);
        if (response.Ok && typeof(TValue).IsValueType)
            return Ok();

        if (response.Ok)
            return Ok(response.Value);


        return BuildErrorResult(response);


    }


    protected virtual IActionResult BuildResult( Response response )
    {

        using var logger = EnterMethod();



        logger.LogObject(nameof(response), response);



        // *****************************************************************
        logger.Debug("Attempting to check for success");
        logger.Inspect(nameof(response.Ok), response.Ok);
        if (response.Ok)
            return Ok();


        return BuildErrorResult(response);


    }


}