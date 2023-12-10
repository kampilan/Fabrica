
// ReSharper disable UnusedMember.Global

using Fabrica.Api.Support.Models;
using Fabrica.Mediator;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Fabrica.Api.Support.Endpoints;


public abstract class RpcEndpointModule<TRequest> : BaseEndpointModule<RpcEndpointModule<TRequest>> where TRequest : class, IRequest<Response>
{


    protected RpcEndpointModule(string route, string summary, string description)
    {
        Route = route;
        Summary = summary;
        Description = description;
    }


    private string Route { get; }
    private string Summary { get; }
    private string Description { get; }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        WithGroupName("Rpc");
        WithTags("Rpc");

        app.MapPost(Route, async ([AsParameters] RpcHandler handler) => await handler.Handle())
            .WithTags(Tags)
            .WithGroupName(OpenApiGroupName)
            .WithSummary(Summary)
            .WithDescription(Description)
            .Produces(200)
            .Produces<ErrorResponseModel>(422)
            .WithOpenApi();

    }


    // ReSharper disable once ClassNeverInstantiated.Local
    private class RpcHandler : BaseMediatorEndpointHandler<TRequest>
    {


        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        // ReSharper disable once MemberCanBePrivate.Local
        [FromBody]
        public TRequest RpcRequest { get; set; } = null!;


        protected override Task<TRequest> BuildRequest()
        {

            using var logger = EnterMethod();

            return Task.FromResult(RpcRequest);

        }


    }


}



public abstract class RpcEndpointModule<TRequest, TResponse> : BaseEndpointModule<RpcEndpointModule<TRequest, TResponse>> where TRequest : class, IRequest<Response<TResponse>> where TResponse : class
{


    protected RpcEndpointModule(string route, string summary, string description)
    {

        Route = route;
        Summary = summary;
        Description = description;

    }


    private string Route { get; }
    private string Summary { get; }
    private string Description { get; }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        WithGroupName("Rpc");
        WithTags("Rpc");

        app.MapPost(Route, async ([AsParameters] RpcHandler handler) => await handler.Handle())
            .WithTags(Tags)
            .WithGroupName(OpenApiGroupName)
            .WithSummary(Summary)
            .WithDescription(Description)
            .Produces<TResponse>()
            .Produces<ErrorResponseModel>(422)
            .WithOpenApi();

    }


    // ReSharper disable once ClassNeverInstantiated.Local
    private class RpcHandler : BaseMediatorEndpointHandler<TRequest, TResponse>
    {


        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        // ReSharper disable once MemberCanBePrivate.Local
        [FromBody]
        public TRequest RpcRequest { get; set; } = null!;


        protected override Task<TRequest> BuildRequest()
        {

            using var logger = EnterMethod();

            return Task.FromResult(RpcRequest);
                ;

        }


    }


}