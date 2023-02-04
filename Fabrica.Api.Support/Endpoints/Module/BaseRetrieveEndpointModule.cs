using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Humanizer;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Api.Support.Endpoints.Module;

public abstract class BaseRetrieveEndpointModule<TEntity>: BasePersistenceEndpointModule where TEntity : class, IModel
{

    protected BaseRetrieveEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected BaseRetrieveEndpointModule(string route) : base(route)
    {

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("By UID")
            .WithDescription($"Retrieve {typeof(TEntity).Name} by UID")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404);

    }


}