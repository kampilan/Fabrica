
// ReSharper disable UnusedMember.Global

using System.Reflection;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Fabrica.Api.Support.Endpoints;

public abstract class QueryEndpointModule<TCriteria, TExplorer, TEntity> : BasePersistenceEndpointModule<QueryEndpointModule<TCriteria, TExplorer, TEntity>> where TCriteria : BaseCriteria where TExplorer : class, IExplorableModel where TEntity : class, IMutableModel
{

    protected QueryEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }

    protected QueryEndpointModule(string route) : base(route)
    {

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        if( string.IsNullOrWhiteSpace(OpenApiGroupName) )
        {
            var label = ExtractResourceLabel<TEntity>();
            WithGroupName(label);
        }

        if( Tags.Length == 0 )
        {
            var label = ExtractResourceLabel<TEntity>();
            WithTags(label);
        }

        app.MapGet("", async ([AsParameters] QueryHandler<TCriteria, TExplorer> handler) => await handler.Handle())
            .AddMetaData<List<TExplorer>>(OpenApiGroupName, summary:"Using Criteria", description:$"Query {typeof(TEntity).Name.Pluralize()} using Criteria")
            .WithTags(Tags);


    }

}

public abstract class QueryEndpointModule<TExplorer, TEntity> : BasePersistenceEndpointModule<QueryEndpointModule<TExplorer, TEntity>> where TExplorer : class, IExplorableModel where TEntity : class, IMutableModel
{

    protected QueryEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }

    protected QueryEndpointModule(string route) : base(route)
    {
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        if( string.IsNullOrWhiteSpace(OpenApiGroupName) )
        {
            var label = ExtractResourceLabel<TEntity>();
            WithGroupName(label);
        }

        if( Tags.Length == 0 )
        {
            var label = ExtractResourceLabel<TEntity>();
            WithTags(label);
        }

        app.MapGet("", async ([AsParameters] QueryHandler<TExplorer> handler) => await handler.Handle())
            .AddMetaData<List<TExplorer>>(OpenApiGroupName, "Using RQL", $"Query {typeof(TEntity).Name.Pluralize()} using RQL")
            .WithTags(Tags);


    }


}