using System.Net;
using System.Text;
using Fabrica.Mediator;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Fabrica.Api.Support.Endpoints.Module;

public abstract class BaseMediatorEndpointHandler : BaseEndpointHandler
{


    [FromServices]
    public IMessageMediator Mediator { get; set; } = null!;


    protected async Task<IResult> Send<TValue>(IRequest<Response<TValue>> request)
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
        var result = BuildResult(response);


        // *****************************************************************
        return result;

    }


    protected async Task<IResult> Send(IRequest<Response> request)
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



    protected virtual IResult BuildResult<TValue>(Response<TValue> response, HttpStatusCode status = HttpStatusCode.OK)
    {

        if (response.Value is MemoryStream stream)
        {
            using (stream)
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
            var result = Results.Content(json, "application/json", Encoding.UTF8, (int)status);

            return result;

        }

    }


}


public abstract class BaseMediatorEndpointHandler<TRequest, TResponse> : BaseMediatorEndpointHandler where TRequest : class, IRequest<Response<TResponse>> where TResponse : class
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


public abstract class BaseMediatorEndpointHandler<TRequest> : BaseMediatorEndpointHandler where TRequest : class, IRequest<Response>
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

