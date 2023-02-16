
// ReSharper disable UnusedMember.Global

using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Fabrica.Models;
using Swashbuckle.AspNetCore.Annotations;


namespace Fabrica.Api.Support.Endpoints;


public abstract class RootEndpointModule<TCriteria, TExplorer, TDelta, TEntity> : BasePersistenceEndpointModule<RootEndpointModule<TCriteria, TExplorer, TDelta, TEntity>> where TCriteria : BaseCriteria where TExplorer : class, IExplorableModel where TDelta : BaseDelta where TEntity : class, IModel
{

    protected RootEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected RootEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        app.MapGet("", async ([AsParameters] QueryHandler<TCriteria, TExplorer> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Using RQL", description: $"Query {typeof(TExplorer).Name.Pluralize()} using RQL"))
            .Produces<List<TExplorer>>()
            .Produces<ErrorResponseModel>(400);


        app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "By UID", description: $"Retrieve {typeof(TEntity).Name} by UID"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404);


        app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Create", description: $"Create {typeof(TEntity).Name} from delta RTO"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Update", description: $"Update {typeof(TEntity).Name} from delta RTO"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422);


        app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Delete", description: $"Delete {typeof(TEntity).Name} by UID"))
            .Produces(200)
            .Produces<ErrorResponseModel>(404);


        app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Journal", description: $"{typeof(TEntity).Name} Audit Journal for given UID"))
            .Produces<List<AuditJournalModel>>();


        app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Patch", description: $"Apply Patches and Retrieve {typeof(TEntity).Name} by UID"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


    }


}


public abstract class RootEndpointModule<TExplorer, TDelta, TEntity> : BasePersistenceEndpointModule<RootEndpointModule<TExplorer, TDelta, TEntity>> where TExplorer : class, IExplorableModel where TDelta : BaseDelta where TEntity : class, IModel
{

    protected RootEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected RootEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        app.MapGet("", async ([AsParameters] QueryHandler<TExplorer> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Using RQL", description: $"Query {typeof(TExplorer).Name.Pluralize()} using RQL"))
            .Produces<List<TExplorer>>()
            .Produces<ErrorResponseModel>(400);


        app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "By UID", description: $"Retrieve {typeof(TEntity).Name} by UID"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404);


        app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Create", description: $"Create {typeof(TEntity).Name} from delta RTO"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Update", description: $"Update {typeof(TEntity).Name} from delta RTO"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422);


        app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Delete", description: $"Delete {typeof(TEntity).Name} by UID"))
            .Produces(200)
            .Produces<ErrorResponseModel>(404);


        app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Journal", description: $"{typeof(TEntity).Name} Audit Journal for given UID"))
            .Produces<List<AuditJournalModel>>();


        app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Patch", description: $"Apply Patches and Retrieve {typeof(TEntity).Name} by UID"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


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

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected RootEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        app.MapGet("", async ([AsParameters] QueryHandler<TExplorer> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Using RQL", description: $"Query {typeof(TExplorer).Name.Pluralize()} using RQL"))
            .Produces<List<TExplorer>>()
            .Produces<ErrorResponseModel>(400);


        app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "By UID", description: $"Retrieve {typeof(TEntity).Name} by UID"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404);


        app.MapPost("", async ([AsParameters] CreateHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Create", description: $"Create {typeof(TEntity).Name} from delta RTO"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Update", description: $"Update {typeof(TEntity).Name} from delta RTO"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422);


        app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Delete", description: $"Delete {typeof(TEntity).Name} by UID"))
            .Produces(200)
            .Produces<ErrorResponseModel>(404);


        app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Journal", description: $"{typeof(TEntity).Name} Audit Journal for given UID"))
            .Produces<List<AuditJournalModel>>();


        app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Patch", description: $"Apply Patches and Retrieve {typeof(TEntity).Name} by UID"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


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

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected RootEndpointModule(string route) : base(route)
    {

        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        app.MapGet("", async ([AsParameters] QueryHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Using RQL", description: $"Query {typeof(TEntity).Name.Pluralize()} using RQL"))
            .Produces<List<TEntity>>()
            .Produces<ErrorResponseModel>(400);


        app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "By UID", description: $"Retrieve {typeof(TEntity).Name} by UID"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404);


        app.MapPost("", async ([AsParameters] CreateHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Create", description: $"Create {typeof(TEntity).Name} from delta RTO"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Update", description: $"Update {typeof(TEntity).Name} from delta RTO"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422);


        app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Delete", description: $"Delete {typeof(TEntity).Name} by UID"))
            .Produces(200)
            .Produces<ErrorResponseModel>(404);


        app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Journal", description: $"{typeof(TEntity).Name} Audit Journal for given UID"))
            .Produces<List<AuditJournalModel>>();


        app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
            .WithMetadata(new SwaggerOperationAttribute(summary: "Patch", description: $"Apply Patches and Retrieve {typeof(TEntity).Name} by UID"))
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


    }


}




