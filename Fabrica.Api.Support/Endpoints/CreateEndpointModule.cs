using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection;

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

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected CreateEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapPost("", async ([AsParameters] CreateHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Create", description: $"Create {typeof(TEntity).Name} from delta RTO"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);

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

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected CreateEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute { Summary = "Create", Description = $"Create {typeof(TEntity).Name} from delta RTO" })
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);

    }

}