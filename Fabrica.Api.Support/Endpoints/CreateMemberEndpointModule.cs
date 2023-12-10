
// ReSharper disable UnusedMember.Global

using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace Fabrica.Api.Support.Endpoints;

public abstract class CreateMemberEndpointModule<TParent, TEntity> : BasePersistenceEndpointModule<CreateMemberEndpointModule<TParent, TEntity>> where TParent : class, IModel where TEntity : class, IAggregateModel
{

    protected CreateMemberEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TParent>();

        BasePath = $"{prefix}/{resource}";

    }

    protected CreateMemberEndpointModule(string route) : base(route)
    {
    }

    protected string MemberSegment { get; set; } = "";


    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        CheckOpenApiDefaults<TEntity>();


        
        var sb = new StringBuilder();
        sb.Append("{uid}");
        if (!string.IsNullOrWhiteSpace(MemberSegment))
            sb.Append($"/{MemberSegment}");
        else
        {
            var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
            MemberSegment = !string.IsNullOrWhiteSpace(attr?.Member) ? attr.Member : ExtractResource<TEntity>();
        }

        var route = sb.ToString();



        app.MapPost(route, async ([AsParameters] CreateMemberHandler<TParent, TEntity> handler) => await handler.Handle())
            .WithTags(Tags)
            .WithSummary("Create Member")
            .WithDescription($"Create {typeof(TEntity).Name} from delta payload in Parent {typeof(TParent).Name}")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422)
            .WithOpenApi();

    }


}


public abstract class CreateMemberEndpointModule<TParent, TDelta, TEntity> : BasePersistenceEndpointModule<CreateMemberEndpointModule<TParent, TDelta, TEntity>> where TParent : class, IModel where TDelta : BaseDelta where TEntity : class, IAggregateModel
{


    protected CreateMemberEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TParent>();

        BasePath = $"{prefix}/{resource}";

    }

    protected CreateMemberEndpointModule(string route) : base(route)
    {

    }

    protected string MemberSegment { get; set; } = "";


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        CheckOpenApiDefaults<TEntity>();



        var sb = new StringBuilder();
        sb.Append("{uid}");
        if (!string.IsNullOrWhiteSpace(MemberSegment))
            sb.Append($"/{MemberSegment}");
        else
        {
            var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
            MemberSegment = !string.IsNullOrWhiteSpace(attr?.Member) ? attr.Member : ExtractResource<TEntity>();
        }

        var route = sb.ToString();



        app.MapPost(route, async ([AsParameters] CreateMemberHandler<TParent, TDelta, TEntity> handler) => await handler.Handle())
            .WithTags(Tags)
            .WithSummary("Create Member")
            .WithDescription($"Create {typeof(TEntity).Name} from delta RTO in Parent {typeof(TParent).Name}")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422)
            .WithOpenApi();

    }


}


