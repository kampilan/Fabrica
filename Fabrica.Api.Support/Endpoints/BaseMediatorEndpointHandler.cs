using AutoMapper;
using Fabrica.Mediator;
using Fabrica.Watch;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Fabrica.Api.Support.Endpoints;

public abstract class BaseMediatorEndpointHandler : BaseEndpointHandler
{


    [FromServices]
    public IMessageMediator Mediator { get; set; } = null!;

    [FromServices]
    public IEndpointResultBuilder Builder { get; set; } = null!;


    protected virtual Task Validate()
    {
        return Task.CompletedTask;
    }


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
        var result = Builder.Create(response);


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



}


public abstract class BaseMediatorEndpointHandler<TRequest, TResponse> : BaseMediatorEndpointHandler where TRequest : class, IRequest<Response<TResponse>> where TResponse : class
{


    protected abstract Task<TRequest> BuildRequest();


    public override async Task<IResult> Handle()
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to validate");
        await Validate();            


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


public abstract class BaseMediatorEndpointHandler<TRequest,TResponse,TRto> : BaseMediatorEndpointHandler where TRequest : class, IRequest<Response<TResponse>> where TResponse : class where TRto: class
{


    [FromServices] 
    public IMapper Mapper { get; set; } = null!;


    protected abstract Task<TRequest> BuildRequest();


    public override async Task<IResult> Handle()
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to validate");
        await Validate();



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = await BuildRequest();



        // *****************************************************************
        logger.Debug("Attempting to send request via mediator");
        var response = await Mediator.Send(request);

        response.EnsureSuccess();



        // *****************************************************************
        logger.Debug("Attempting to map response to Rto");
        var rto = Mapper.Map<TRto>(response.Value);

        logger.LogObject(nameof(rto), rto);



        // *****************************************************************
        logger.Debug("Attempting to build result");
        var result = Builder.Create(rto);



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
        logger.Debug("Attempting to validate");
        await Validate();



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

