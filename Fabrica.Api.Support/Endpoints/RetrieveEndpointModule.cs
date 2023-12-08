
// ReSharper disable UnusedMember.Global

using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Humanizer;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints;

public abstract class RetrieveEndpointModule<TEntity> : BasePersistenceEndpointModule<RetrieveEndpointModule<TEntity>> where TEntity : class, IModel
{

    protected RetrieveEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

        WithGroupName($"{typeof(TEntity).Name.Pluralize().Humanize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");
    }

    protected RetrieveEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize().Humanize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
            .AddMetaData<TEntity>(OpenApiGroupName, summary: "By UID", description: $"Retrieve {typeof(TEntity).Name} by UID")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404);

    }


}