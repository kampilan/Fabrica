﻿
// ReSharper disable UnusedMember.Global

using System.Reflection;
using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fabrica.Api.Support.Endpoints;


public abstract class DeltaEndpointModule<TEntity> : BasePersistenceEndpointModule<DeltaEndpointModule<TEntity>> where TEntity : class, IModel
{


    protected DeltaEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }

    protected DeltaEndpointModule(string route) : base(route)
    {

    }

    protected bool IncludeCreateEndpoint { get; set; } = true;
    protected bool IncludeUpdateEndpoint { get; set; } = true;
    protected bool IncludeDeleteEndpoint { get; set; } = true;


    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        CheckOpenApiDefaults<TEntity>();


        if (IncludeCreateEndpoint)
        {
            app.MapPost("", async ([AsParameters] CreateHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithSummary("Create")
                .WithDescription($"Create {typeof(TEntity).Name} from delta RTO")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(422)
                .WithOpenApi();
        }

        if (IncludeUpdateEndpoint)
        {
            app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithSummary("Update")
                .WithDescription($"Update {typeof(TEntity).Name} from delta RTO")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(404)
                .Produces<ErrorResponseModel>(422)
                .WithOpenApi();
        }

        if (IncludeDeleteEndpoint)
        {

            app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithSummary("Delete")
                .WithDescription($"Delete {typeof(TEntity).Name} by UID")
                .Produces(200)
                .Produces<ErrorResponseModel>(404)
                .WithOpenApi();
        }

    }


}


public abstract class DeltaEndpointModule<TDelta, TEntity> : BasePersistenceEndpointModule<DeltaEndpointModule<TDelta, TEntity>> where TDelta : BaseDelta where TEntity : class, IModel
{


    protected DeltaEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }


    protected DeltaEndpointModule(string route) : base(route)
    {
    }

    protected bool IncludeCreateEndpoint { get; set; } = true;
    protected bool IncludeUpdateEndpoint { get; set; } = true;
    protected bool IncludeDeleteEndpoint { get; set; } = true;



    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        CheckOpenApiDefaults<TEntity>();


        if (IncludeCreateEndpoint)
        {
            app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithSummary("Create")
                .WithDescription($"Create {typeof(TEntity).Name} from delta RTO")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(422)
                .WithOpenApi();
        }

        if (IncludeUpdateEndpoint)
        {
            app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithSummary("Update")
                .WithDescription($"Update {typeof(TEntity).Name} from delta RTO")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(404)
                .Produces<ErrorResponseModel>(422)
                .WithOpenApi();
        }

        if (IncludeDeleteEndpoint)
        {
            app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithSummary("Delete")
                .WithDescription($"Delete {typeof(TEntity).Name} using UID")
                .Produces(200)
                .Produces<ErrorResponseModel>(404)
                .WithOpenApi();
        }

    }


}