
// ReSharper disable UnusedMember.Global

using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Api.Support.Endpoints;

public abstract class CreateMemberEndpointModule<TParent, TEntity> : BasePersistenceEndpointModule<CreateMemberEndpointModule<TParent, TEntity>> where TParent : class, IModel where TEntity : class, IAggregateModel
{

    protected CreateMemberEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TParent>();

        BasePath = $"{prefix}/{resource}";

        MemberSegment = !string.IsNullOrWhiteSpace(attr?.Member) ? attr.Member : ExtractResource<TEntity>();


        WithGroupName($"{typeof(TEntity).Name.Pluralize().Humanize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");


    }

    protected CreateMemberEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize().Humanize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected string MemberSegment { get; set; } = "";


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        var sb = new StringBuilder();
        sb.Append("{uid}");
        if (!string.IsNullOrWhiteSpace(MemberSegment))
            sb.Append($"/{MemberSegment}");

        var route = sb.ToString();

        app.MapPost(route, async ([AsParameters] CreateMemberHandler<TParent, TEntity> handler) => await handler.Handle())
            .AddMetaData<TEntity>(OpenApiGroupName, summary: "Create Member", description: $"Create {typeof(TEntity).Name} from delta RTO in Parent {typeof(TParent).Name}")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);

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

        MemberSegment = !string.IsNullOrWhiteSpace(attr?.Member) ? attr.Member : ExtractResource<TEntity>();

        WithGroupName($"{typeof(TEntity).Name.Pluralize().Humanize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected CreateMemberEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize().Humanize()}");
        WithTags($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected string MemberSegment { get; set; } = "";


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        var sb = new StringBuilder();
        sb.Append("{uid}");
        if (!string.IsNullOrWhiteSpace(MemberSegment))
            sb.Append($"/{MemberSegment}");

        var route = sb.ToString();

        app.MapPost(route, async ([AsParameters] CreateMemberHandler<TParent, TDelta, TEntity> handler) => await handler.Handle())
            .AddMetaData<TEntity>(OpenApiGroupName, summary: "Create Member", description: $"Create {typeof(TEntity).Name} from delta RTO in Parent {typeof(TParent).Name}")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);

    }


}


