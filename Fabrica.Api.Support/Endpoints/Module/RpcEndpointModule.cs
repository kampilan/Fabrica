﻿
// ReSharper disable UnusedMember.Global

using Fabrica.Api.Support.Models;
using Fabrica.Mediator;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints.Module;

public abstract class RpcEndpointModule<TRequest,TResponse>: BaseEndpointModule where TRequest : class, IRequest<Response<TResponse>> where TResponse: class
{


    protected RpcEndpointModule(string route, string summary, string description) : base()
    {

        Route       = route;
        Summary     = summary;
        Description = description;

    }


    private string Route { get; }
    private string Summary { get; }
    private string Description { get; }


    public override void AddRoutes( IEndpointRouteBuilder app )
    {

        app.MapPost( Route, async ([AsParameters] RpcHandler handler) => await handler.Handle() )
            .WithMetadata(new SwaggerOperationAttribute { Summary = Summary, Description = Description })
            .Produces<TResponse>()
            .Produces<ErrorResponseModel>(422);

    }


    // ReSharper disable once ClassNeverInstantiated.Local
    private class RpcHandler : BaseMediatorEndpointHandler<TRequest, TResponse>
    {


        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        [FromBody] 
        private TRequest RpcRequest { get; set; } = null!;


        protected override Task<TRequest> BuildRequest()
        {
            
            using var logger = EnterMethod();

            return Task.FromResult(RpcRequest);

        }


    }


}