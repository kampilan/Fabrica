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

public class UpdateEndpointModule<TEntity> : BasePersistenceEndpointModule<UpdateEndpointModule<TEntity>> where TEntity : class, IModel
{

    protected UpdateEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected UpdateEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        
        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Update", description: $"Update {typeof(TEntity).Name} from delta RTO"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422);

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

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected UpdateEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute { Summary = "Update", Description = $"Update {typeof(TEntity).Name} from delta RTO" })
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422);

    }


}