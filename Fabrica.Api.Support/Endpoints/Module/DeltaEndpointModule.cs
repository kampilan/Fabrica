
// ReSharper disable UnusedMember.Global

using System.Reflection;
using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints.Module;


public abstract class DeltaEndpointModule<TEntity> : BasePersistenceEndpointModule where TEntity : class, IModel
{


    protected DeltaEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";


        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected DeltaEndpointModule(string route) : base(route)
    {

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapPost("", async ([AsParameters] CreateHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Create", description: $"Create {typeof(TEntity).Name} from delta RTO"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);

        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Update", description: $"Update {typeof(TEntity).Name} from delta RTO"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422);


        app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Delete", description: $"Delete {typeof(TEntity).Name} by UID"))
            .Produces(200)
            .Produces<ErrorResponseModel>(404);

    }


}


public abstract class DeltaEndpointModule<TDelta, TEntity> : BasePersistenceEndpointModule where TDelta : BaseDelta where TEntity : class, IModel
{


    protected DeltaEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";


        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }


    protected DeltaEndpointModule(string route) : base(route)
    {

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithMetadata( new SwaggerOperationAttribute{Summary = "Create", Description = $"Create {typeof(TEntity).Name} from delta RTO" })
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);

        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute { Summary = "Update", Description = $"Update {typeof(TEntity).Name} from delta RTO" })
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422);


        app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute { Summary = "Delete", Description = $"Delete {typeof(TEntity).Name} using UID" })
            .Produces(200)
            .Produces<ErrorResponseModel>(404);

    }



}