using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using Swashbuckle.AspNetCore.Annotations;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Api.Support.Endpoints;

public class CreateEndpointModule<TEntity> : BasePersistenceEndpointModule<CreateEndpointModule<TEntity>> where TEntity : class, IModel
{


    protected CreateEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }

    protected CreateEndpointModule(string route) : base(route)
    {
    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        CheckOpenApiDefaults<TEntity>();


        app.MapPost("", async ([AsParameters] CreateHandler<TEntity> handler) => await handler.Handle())
            .WithName($"Create{typeof(TEntity).Name}")
            .WithTags(Tags)
            .WithSummary("Create")
            .WithDescription($"Create {typeof(TEntity).Name} from delta payload")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422)
            .WithOpenApi();



    }

}


public class CreateEndpointModule<TDelta, TEntity> : BasePersistenceEndpointModule<CreateEndpointModule<TDelta, TEntity>> where TDelta : BaseDelta where TEntity : class, IModel
{


    protected CreateEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }

    protected CreateEndpointModule(string route) : base(route)
    {

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        CheckOpenApiDefaults<TEntity>();

        app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithName($"Create{typeof(TEntity).Name}")
            .WithTags(Tags)
            .WithSummary("Create")
            .WithDescription($"Create {typeof(TEntity).Name} from delta RTO")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422)
            .WithOpenApi();

    }


}