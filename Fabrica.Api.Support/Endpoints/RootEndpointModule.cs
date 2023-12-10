
// ReSharper disable UnusedMember.Global

using Fabrica.Models.Support;
using Fabrica.Rql;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using Fabrica.Api.Support.Models;
using Microsoft.AspNetCore.Builder;
using Fabrica.Models;


namespace Fabrica.Api.Support.Endpoints;


public abstract class RootEndpointModule<TCriteria, TExplorer, TDelta, TEntity> : BasePersistenceEndpointModule<RootEndpointModule<TCriteria,TExplorer,TDelta,TEntity>> where TCriteria : BaseCriteria where TExplorer : class, IExplorableModel where TDelta : BaseDelta where TEntity : class, IModel
{

    protected RootEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }

    protected RootEndpointModule(string route) : base(route)
    {
    }

    protected bool IncludeQueryEndpoint { get; set; } = true;
    protected bool IncludeRetrieveEndpoint { get; set; } = true;
    protected bool IncludeCreateEndpoint { get; set; } = true;
    protected bool IncludeUpdateEndpoint { get; set; } = true;
    protected bool IncludeDeleteEndpoint { get; set; } = true;
    protected bool IncludeJournalEndpoint { get; set; } = true;
    protected bool IncludePatchEndpoint { get; set; } = true;


    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        CheckOpenApiDefaults<TEntity>();


        if (IncludeQueryEndpoint)
        {
            app.MapGet("", async ([AsParameters] QueryHandler<TExplorer> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Using Criteria")
                .WithDescription($"Query {typeof(TEntity).Name.Pluralize()} using Criteria")
                .Produces<List<TExplorer>>();

        }

        if (IncludeRetrieveEndpoint)
        {
            app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("By UID")
                .WithDescription($"Retrieve {typeof(TEntity).Name} by UID")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(404);

        }

        if (IncludeCreateEndpoint)
        {
            app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Create")
                .WithDescription($"Create {typeof(TEntity).Name} from delta RTO")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(422);

        }

        if (IncludeUpdateEndpoint)
        {
            app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Update")
                .WithDescription($"Update {typeof(TEntity).Name} from delta RTO")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(404)
                .Produces<ErrorResponseModel>(422);

        }

        if (IncludeDeleteEndpoint)
        {
            app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Delete")
                .WithDescription($"Delete {typeof(TEntity).Name} by UID")
                .Produces(200)
                .Produces<ErrorResponseModel>(404);

        }

        if (IncludeJournalEndpoint)
        {
            app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Journal")
                .WithDescription($"{typeof(TEntity).Name} Audit Journal for given UID")
                .Produces<List<AuditJournalModel>>();
        }

        if (IncludePatchEndpoint)
        {
            app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Patch")
                .WithDescription($"Apply Patches and Retrieve {typeof(TEntity).Name} by UID")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(422);

        }


    }


}


public abstract class RootEndpointModule<TExplorer,TDelta,TEntity> : BasePersistenceEndpointModule<RootEndpointModule<TExplorer,TDelta,TEntity>> where TExplorer : class, IExplorableModel where TDelta : BaseDelta where TEntity : class, IModel
{

    protected RootEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";


    }

    protected RootEndpointModule(string route) : base(route)
    {
    }


    protected bool IncludeQueryEndpoint { get; set; } = true;
    protected bool IncludeRetrieveEndpoint { get; set; } = true;
    protected bool IncludeCreateEndpoint { get; set; } = true;
    protected bool IncludeUpdateEndpoint { get; set; } = true;
    protected bool IncludeDeleteEndpoint { get; set; } = true;
    protected bool IncludeJournalEndpoint { get; set; } = true;
    protected bool IncludePatchEndpoint { get; set; } = true;



    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        CheckOpenApiDefaults<TEntity>();


        if (IncludeQueryEndpoint)
        {
            app.MapGet("", async ([AsParameters] QueryHandler<TExplorer> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Using RQL")
                .WithDescription($"Query {typeof(TEntity).Name.Pluralize()} using RQL")
                .Produces<List<TExplorer>>();

        }

        if (IncludeRetrieveEndpoint)
        {
            app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("By UID")
                .WithDescription($"Retrieve {typeof(TEntity).Name} by UID")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(404);

        }

        if (IncludeCreateEndpoint)
        {
            app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Create")
                .WithDescription($"Create {typeof(TEntity).Name} from delta RTO")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(422);

        }

        if (IncludeUpdateEndpoint)
        {
            app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Update")
                .WithDescription($"Update {typeof(TEntity).Name} from delta RTO")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(404)
                .Produces<ErrorResponseModel>(422);

        }

        if (IncludeDeleteEndpoint)
        {
            app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Delete")
                .WithDescription($"Delete {typeof(TEntity).Name} by UID")
                .Produces(200)
                .Produces<ErrorResponseModel>(404);

        }

        if (IncludeJournalEndpoint)
        {
            app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Journal")
                .WithDescription($"{typeof(TEntity).Name} Audit Journal for given UID")
                .Produces<List<AuditJournalModel>>();
        }

        if (IncludePatchEndpoint)
        {
            app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Patch")
                .WithDescription($"Apply Patches and Retrieve {typeof(TEntity).Name} by UID")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(422);

        }



    }


}


public abstract class RootEndpointModule<TExplorer, TEntity> : BasePersistenceEndpointModule<RootEndpointModule<TExplorer, TEntity>> where TExplorer : class, IExplorableModel where TEntity : class, IModel
{

    protected RootEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }

    protected RootEndpointModule(string route) : base(route)
    {

    }


    protected bool IncludeQueryEndpoint { get; set; } = true;
    protected bool IncludeRetrieveEndpoint { get; set; } = true;
    protected bool IncludeCreateEndpoint { get; set; } = true;
    protected bool IncludeUpdateEndpoint { get; set; } = true;
    protected bool IncludeDeleteEndpoint { get; set; } = true;
    protected bool IncludeJournalEndpoint { get; set; } = true;
    protected bool IncludePatchEndpoint { get; set; } = true;



    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        CheckOpenApiDefaults<TEntity>();


        if (IncludeQueryEndpoint)
        {
            app.MapGet("", async ([AsParameters] QueryHandler<TExplorer> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Using RQL")
                .WithDescription($"Query {typeof(TEntity).Name.Pluralize()} using RQL")
                .Produces<List<TExplorer>>();

        }

        if (IncludeRetrieveEndpoint)
        {
            app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("By UID")
                .WithDescription($"Retrieve {typeof(TEntity).Name} by UID")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(404);

        }

        if (IncludeCreateEndpoint)
        {
            app.MapPost("", async ([AsParameters] CreateHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Create")
                .WithDescription($"Create {typeof(TEntity).Name} from delta payload")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(422);

        }

        if (IncludeUpdateEndpoint)
        {
            app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Update")
                .WithDescription($"Update {typeof(TEntity).Name} from delta payload")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(404)
                .Produces<ErrorResponseModel>(422);

        }

        if (IncludeDeleteEndpoint)
        {
            app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Delete")
                .WithDescription($"Delete {typeof(TEntity).Name} by UID")
                .Produces(200)
                .Produces<ErrorResponseModel>(404);

        }

        if (IncludeJournalEndpoint)
        {
            app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Journal")
                .WithDescription($"{typeof(TEntity).Name} Audit Journal for given UID")
                .Produces<List<AuditJournalModel>>();
        }


        if (IncludePatchEndpoint)
        {
            app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Patch")
                .WithDescription($"Apply Patches and Retrieve {typeof(TEntity).Name} by UID")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(422);

        }


    }


}


public abstract class RootEndpointModule<TEntity> : BasePersistenceEndpointModule<RootEndpointModule<TEntity>> where TEntity : class, IExplorableModel
{

    protected RootEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

    }

    protected RootEndpointModule(string route) : base(route)
    {
    }

    protected bool IncludeQueryEndpoint { get; set; } = true;
    protected bool IncludeRetrieveEndpoint { get; set; } = true;
    protected bool IncludeCreateEndpoint { get; set; } = true;
    protected bool IncludeUpdateEndpoint { get; set; } = true;
    protected bool IncludeDeleteEndpoint { get; set; } = true;
    protected bool IncludeJournalEndpoint { get; set; } = true;
    protected bool IncludePatchEndpoint { get; set; } = true;


    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        CheckOpenApiDefaults<TEntity>();


        if (IncludeQueryEndpoint)
        {
            app.MapGet("", async ([AsParameters] QueryHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Using RQL")
                .WithDescription($"Query {typeof(TEntity).Name.Pluralize()} using RQL")
                .Produces<List<TEntity>>();

        }

        if (IncludeRetrieveEndpoint)
        {
            app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("By UID")
                .WithDescription($"Retrieve {typeof(TEntity).Name} by UID")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(404);

        }

        if (IncludeCreateEndpoint)
        {
            app.MapPost("", async ([AsParameters] CreateHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Create")
                .WithDescription($"Create {typeof(TEntity).Name} from delta payload")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(422);

        }

        if (IncludeUpdateEndpoint)
        {
            app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Update")
                .WithDescription($"Update {typeof(TEntity).Name} from delta payload")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(404)
                .Produces<ErrorResponseModel>(422);

        }

        if (IncludeDeleteEndpoint)
        {
            app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Delete")
                .WithDescription($"Delete {typeof(TEntity).Name} by UID")
                .Produces(200)
                .Produces<ErrorResponseModel>(404);

        }

        if (IncludeJournalEndpoint)
        {
            app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Journal")
                .WithDescription($"{typeof(TEntity).Name} Audit Journal for given UID")
                .Produces<List<AuditJournalModel>>();
        }


        if (IncludePatchEndpoint)
        {
            app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
                .WithTags(Tags)
                .WithGroupName(OpenApiGroupName)
                .WithSummary("Patch")
                .WithDescription($"Apply Patches and Retrieve {typeof(TEntity).Name} by UID")
                .Produces<TEntity>()
                .Produces<ErrorResponseModel>(422);

        }


    }


}




