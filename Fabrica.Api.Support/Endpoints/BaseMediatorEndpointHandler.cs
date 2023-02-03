using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using Fabrica.Api.Support.Models;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.Api.Support.Endpoints;

public abstract class BaseEndpointHandler
{


    [FromServices]
    public ICorrelation Correlation { get; set; } = null!;

    protected ILogger EnterMethod([CallerMemberName] string name = "")
    {
        var logger = Correlation.EnterMethod(GetType(), name);
        return logger;
    }


    public HttpContext Context { get; set; } = null!;


    public HttpRequest Request => Context.Request;
    public HttpResponse Response => Context.Response;
    public ClaimsPrincipal User => Context.User;


    public abstract Task<IResult> Handle();


    protected virtual bool TryValidate(BaseCriteria? criteria, out IResult error)
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(criteria), criteria);

        error = null!;



        if (criteria is null)
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "CriteriaInvalid",
                Explanation = $"Errors occurred while parsing criteria for {Request.Method} at {Request.Path}"
            };

            error = BuildErrorResult(info);

            return false;

        }



        if (criteria.IsOverposted())
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "DisallowedProperties",
                Explanation = $"The following properties were not found: ({string.Join(',', criteria.GetOverpostNames())})"
            };

            error = BuildErrorResult(info);

            return false;

        }


        return true;


    }


    protected virtual bool TryValidate(BaseDelta? delta, out IResult error)
    {

        using var logger = EnterMethod();

        logger.LogObject(nameof(delta), delta);

        error = null!;


        if (delta is null)
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "ModelInvalid",
                Explanation = $"Errors occurred while parsing model for {Request.Method} at {Request.Path}"
            };

            error = BuildErrorResult(info);

            return false;

        }



        if (delta.IsOverposted())
        {

            var info = new ExceptionInfoModel
            {
                Kind = ErrorKind.BadRequest,
                ErrorCode = "DisallowedProperties",
                Explanation = $"The following properties were not found or are not mutable: ({string.Join(',', delta.GetOverpostNames())})"
            };

            error = BuildErrorResult(info);

            return false;

        }


        return true;


    }



    protected async Task<string> FromBody()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        return body;
    }

    protected async Task<TTarget> FromBody<TTarget>() where TTarget : class
    {

        using var logger = EnterMethod();

        var serializer = JsonSerializer.Create(BaseEndpointModule.Settings);

        using var reader = new StreamReader(Request.Body);
        await using var jreader = new JsonTextReader(reader);

        var token = await JToken.LoadAsync(jreader);
        if (token is null)
            throw new BadRequestException($"Could not parse JSON body in {GetType().FullName}.FromBody<>");

        var target = token.ToObject<TTarget>(serializer);
        if (target is null)
            throw new BadRequestException($"Could not parse Token in {GetType().FullName}.FromBody<>");


        return target;

    }


    protected virtual IResult BuildResult(object model, HttpStatusCode status = HttpStatusCode.OK)
    {

        var json = JsonConvert.SerializeObject(model, BaseEndpointModule.Settings);
        var result = Results.Content(json, "application/json", Encoding.UTF8, (int)status);

        return result;

    }


    protected IResult BuildErrorResult(IExceptionInfo error)
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
        var result = BuildResult(model, status);



        // *****************************************************************
        return result;


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


}


public abstract class BaseMediatorEndpointHandler: BaseEndpointHandler
{


    [FromServices]
    public IMessageMediator Mediator { get; set; } = null!;


    protected async Task<IResult> Send<TValue>( IRequest<Response<TValue>> request )
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to Send request via Mediator");
        var response = await Mediator.Send( request );



        // *****************************************************************
        logger.Debug("Attempting to check for success");
        response.EnsureSuccess();


        // *****************************************************************
        logger.Debug("Attempting to build Result");
        var result = BuildResult(response);


        // *****************************************************************
        return result;

    }


    protected async Task<IResult> Send( IRequest<Mediator.Response> request )
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to Send request via Mediator");
        var response = await Mediator.Send(request);



        // *****************************************************************
        logger.Debug("Attempting to check for success");
        response.EnsureSuccess();


        // *****************************************************************
        logger.Debug("Attempting to build Result");
        var result = Results.Ok();


        // *****************************************************************
        return result;

    }



    protected virtual IResult BuildResult<TValue>( Response<TValue> response, HttpStatusCode status=HttpStatusCode.OK )
    {

        if( response.Value is MemoryStream stream )
        {
            using( stream )
            {

                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();

                var result = Results.Content(json, "application/json", Encoding.UTF8, (int)status);

                return result;

            }

        }
        // ReSharper disable once RedundantIfElseBlock
        else
        {

            var json = JsonConvert.SerializeObject(response.Value, BaseEndpointModule.Settings);
            var result = Results.Content(json, "application/json", Encoding.UTF8, (int) status);

            return result;

        }

    }


}


public abstract class BaseMediatorEndpointHandler<TRequest,TResponse>: BaseMediatorEndpointHandler where TRequest : class, IRequest<Response<TResponse>> where TResponse: class
{


    protected abstract Task<TRequest> BuildRequest();


    public override async Task<IResult> Handle()
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = await BuildRequest();


        // *****************************************************************
        logger.Debug("Attempting to send request via mediator");
        var result = await Send(request);


        // *****************************************************************
        return result;

    }


}


public abstract class BaseMediatorEndpointHandler<TRequest> : BaseMediatorEndpointHandler where TRequest : class, IRequest<Mediator.Response>
{


    protected abstract Task<TRequest> BuildRequest();


    public override async Task<IResult> Handle()
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = await BuildRequest();


        // *****************************************************************
        logger.Debug("Attempting to send request via mediator");
        var result = await Send(request);


        // *****************************************************************
        return result;

    }




}

