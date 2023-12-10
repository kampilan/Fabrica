
// ReSharper disable UnusedMember.Global

using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Api.Support.Endpoints;

public abstract class RetrieveEndpointModule<TEntity> : BasePersistenceEndpointModule<RetrieveEndpointModule<TEntity>> where TEntity : class, IModel
{

    protected RetrieveEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";
    }

    protected RetrieveEndpointModule(string route) : base(route)
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        CheckOpenApiDefaults<TEntity>();


        app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
            .WithTags(Tags)
            .WithSummary("By UID")
            .WithDescription($"Retrieve {typeof(TEntity).Name} by UID")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .WithOpenApi();

    }


}