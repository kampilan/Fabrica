
// ReSharper disable UnusedMember.Global

using System.Reflection;
using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fabrica.Api.Support.Endpoints.Module;

public abstract class BaseQueryEndpointModule<TCriteria, TExplorer, TEntity> : BasePersistenceEndpointModule where TCriteria : BaseCriteria where TExplorer : class, IExplorableModel where TEntity : class, IMutableModel
{

    protected BaseQueryEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected BaseQueryEndpointModule(string route) : base(route)
    {

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        app.MapGet("", async ([AsParameters] QueryHandler<TCriteria, TExplorer> handler) => await handler.Handle())
            .WithSummary("Using RQL")
            .WithDescription($"Query {typeof(TExplorer).Name.Pluralize()} using RQL")
            .Produces<List<TExplorer>>()
            .Produces<ErrorResponseModel>(400);

    }

}

public abstract class BaseQueryEndpointModule<TExplorer, TEntity> : BasePersistenceEndpointModule where TExplorer : class, IExplorableModel where TEntity : class, IMutableModel
{

    protected BaseQueryEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");
        IncludeInOpenApi();

    }

    protected BaseQueryEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");
        IncludeInOpenApi();

    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapGet("", async ([AsParameters] QueryHandler<TExplorer> handler) => await handler.Handle())
            .WithSummary("Using RQL")
            .WithDescription($"Query {typeof(TExplorer).Name.Pluralize()} using RQL")
            .Produces<List<TExplorer>>()
            .Produces<ErrorResponseModel>(400);

    }


}