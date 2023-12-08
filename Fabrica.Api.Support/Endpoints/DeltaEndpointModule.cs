
// ReSharper disable UnusedMember.Global

using System.Reflection;
using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Humanizer;
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

        WithGroupName($"{typeof(TEntity).Name.Pluralize().Humanize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected DeltaEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize().Humanize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected bool IncludeCreateEndpoint { get; set; } = true;
    protected bool IncludeUpdateEndpoint { get; set; } = true;
    protected bool IncludeDeleteEndpoint { get; set; } = true;


    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        if (IncludeCreateEndpoint)
        {
            app.MapPost("", async ([AsParameters] CreateHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(OpenApiGroupName, summary: "Create", description: $"Create {typeof(TEntity).Name} from delta RTO")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(422);
        }

        if (IncludeUpdateEndpoint)
        {
            app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(OpenApiGroupName, summary: "Update", description: $"Update {typeof(TEntity).Name} from delta RTO")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(404)
                .Produces<ErrorResponseModel>(422);
        }

        if (IncludeDeleteEndpoint)
        {

            app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(OpenApiGroupName, summary: "Delete", description: $"Delete {typeof(TEntity).Name} by UID")
                .Produces(200)
                .Produces<ErrorResponseModel>(404);
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

        WithGroupName($"{typeof(TEntity).Name.Pluralize().Humanize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }


    protected DeltaEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize().Humanize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected bool IncludeCreateEndpoint { get; set; } = true;
    protected bool IncludeUpdateEndpoint { get; set; } = true;
    protected bool IncludeDeleteEndpoint { get; set; } = true;



    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        if (IncludeCreateEndpoint)
        {
            app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(OpenApiGroupName, summary: "Create", description: $"Create {typeof(TEntity).Name} from delta RTO")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(422);
        }

        if (IncludeUpdateEndpoint)
        {
            app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(OpenApiGroupName, summary: "Update", description: $"Update {typeof(TEntity).Name} from delta RTO")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(404)
                .Produces<ErrorResponseModel>(422);
        }

        if (IncludeDeleteEndpoint)
        {
            app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(OpenApiGroupName, summary: "Delete", description: $"Delete {typeof(TEntity).Name} using UID")
                .Produces(200)
                .Produces<ErrorResponseModel>(404);
        }

    }


}