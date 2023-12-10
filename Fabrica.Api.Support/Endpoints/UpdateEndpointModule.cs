using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Api.Support.Endpoints;

public class UpdateEndpointModule<TEntity> : BasePersistenceEndpointModule<UpdateEndpointModule<TEntity>> where TEntity : class, IModel
{

    protected UpdateEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }

    protected UpdateEndpointModule(string route) : base(route)
    {
    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        CheckOpenApiDefaults<TEntity>();


        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TEntity> handler) => await handler.Handle())
            .WithTags(Tags)
            .WithGroupName(OpenApiGroupName)
            .WithSummary("Update")
            .WithDescription($"Update {typeof(TEntity).Name} from delta payload")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422)
            .WithOpenApi();

    }

}


public class UpdateEndpointModule<TDelta, TEntity> : BasePersistenceEndpointModule<UpdateEndpointModule<TDelta, TEntity>> where TDelta : BaseDelta where TEntity : class, IModel
{

    protected UpdateEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }

    protected UpdateEndpointModule(string route) : base(route)
    {
    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        CheckOpenApiDefaults<TEntity>();


        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithTags(Tags)
            .WithGroupName(OpenApiGroupName)
            .WithSummary("Update")
            .WithDescription($"Update {typeof(TEntity).Name} from delta RTO")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422)
            .WithOpenApi();

    }


}