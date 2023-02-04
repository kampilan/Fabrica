
// ReSharper disable UnusedMember.Global

using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Fabrica.Api.Support.Endpoints.Module;

public abstract class BaseCreateMemberEndpointModule<TParent,TEntity>: BasePersistenceEndpointModule where TParent : class, IModel where TEntity : class, IAggregateModel
{


    protected BaseCreateMemberEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TParent>();

        BasePath = $"{prefix}/{resource}";

        MemberSegment = !string.IsNullOrWhiteSpace(attr?.Member) ? attr.Member : ExtractResource<TEntity>();


        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");


    }

    protected BaseCreateMemberEndpointModule(string route) : base(route)
    {

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected string MemberSegment { get; set; } = "";


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        var route = $"{{udi}}/{MemberSegment}";

        app.MapPost(route, async ([AsParameters] CreateMemberHandler<TParent,TEntity> handler) => await handler.Handle())
            .WithSummary("Create")
            .WithDescription( $"Create {typeof(TEntity).Name} on Parent {typeof(TParent).Name}" )
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);

    }


}


public abstract class BaseCreateMemberEndpointModule<TParent,TDelta,TEntity> : BasePersistenceEndpointModule where TParent : class, IModel where TDelta: BaseDelta where TEntity : class, IAggregateModel
{


    protected BaseCreateMemberEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TParent>();

        BasePath = $"{prefix}/{resource}";

        MemberSegment = !string.IsNullOrWhiteSpace(attr?.Member) ? attr.Member : ExtractResource<TEntity>();


        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");


    }

    protected BaseCreateMemberEndpointModule(string route) : base(route)
    {

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected string MemberSegment { get; set; } = "";


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        var route = $"{{udi}}/{MemberSegment}";

        app.MapPost(route, async ([AsParameters] CreateMemberHandler<TParent,TDelta,TEntity> handler) => await handler.Handle())
            .WithSummary("Create")
            .WithDescription($"Create {typeof(TEntity).Name} on Parent {typeof(TParent).Name}")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);

    }


}


