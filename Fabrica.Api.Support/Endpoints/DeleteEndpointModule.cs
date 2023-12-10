using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Api.Support.Endpoints;

public class DeleteEndpointModule<TEntity> : BasePersistenceEndpointModule<DeleteEndpointModule<TEntity>> where TEntity : class, IModel
{


    protected DeleteEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }

    protected DeleteEndpointModule(string route) : base(route)
    {

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        CheckOpenApiDefaults<TEntity>();

        app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
            .WithTags(Tags)
            .WithSummary("Delete")
            .WithDescription($"Delete {typeof(TEntity).Name} by UID")
            .Produces(200)
            .Produces<ErrorResponseModel>(404)
            .WithOpenApi();

    }


}