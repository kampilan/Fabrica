using Fabrica.Api.Support.Models;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Fabrica.Models;

namespace Fabrica.Api.Support.Endpoints.Module;


public abstract class BaseModelEndpointModule<TCriteria,TExplorer,TDelta,TEntity>: BasePersistenceEndpointModule where TCriteria: BaseCriteria where TExplorer: class, IExplorableModel where TDelta: BaseDelta where TEntity: class, IModel
{

    protected BaseModelEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";


        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected BaseModelEndpointModule(string route) : base(route)
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


        app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("By UID")
            .WithDescription($"Retrieve {typeof(TEntity).Name} by UID")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404);


        app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithSummary("Create")
            .WithDescription($"Create {typeof(TEntity).Name} from delta RTO")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithSummary("Update")
            .WithDescription($"Update {typeof(TEntity).Name} from delta RTO")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422);


        app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("Delete")
            .WithDescription($"Delete {typeof(TEntity).Name} using UID")
            .Produces(200)
            .Produces<ErrorResponseModel>(404);


        app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("Journal")
            .WithDescription($"{typeof(TEntity).Name} Audit Journal for given UID")
            .Produces<List<AuditJournalModel>>();


        app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("Patch")
            .WithDescription($"Apply Patches and Retrieve {typeof(TEntity).Name} but UID")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


    }


}

public abstract class BaseModelEndpointModule<TExplorer, TDelta, TEntity> : BasePersistenceEndpointModule where TExplorer : class, IExplorableModel where TDelta : BaseDelta where TEntity : class, IModel
{

    protected BaseModelEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";


        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected BaseModelEndpointModule(string route) : base(route)
    {

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        app.MapGet("", async ([AsParameters] QueryHandler<TExplorer> handler) => await handler.Handle())
            .WithSummary("Using RQL")
            .WithDescription($"Query {typeof(TExplorer).Name.Pluralize()} using RQL")
            .Produces<List<TExplorer>>()
            .Produces<ErrorResponseModel>(400);


        app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("By UID")
            .WithDescription($"Retrieve {typeof(TEntity).Name} by UID")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404);


        app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithSummary("Create")
            .WithDescription($"Create {typeof(TEntity).Name} from delta RTO")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
            .WithSummary("Update")
            .WithDescription($"Update {typeof(TEntity).Name} from delta RTO")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422);


        app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("Delete")
            .WithDescription($"Delete {typeof(TEntity).Name} using UID")
            .Produces(200)
            .Produces<ErrorResponseModel>(404);


        app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("Journal")
            .WithDescription($"{typeof(TEntity).Name} Audit Journal for given UID")
            .Produces<List<AuditJournalModel>>();


        app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("Patch")
            .WithDescription($"Apply Patches and Retrieve {typeof(TEntity).Name} but UID")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


    }


}


public abstract class BaseModelEndpointModule<TExplorer,TEntity> : BasePersistenceEndpointModule where TExplorer : class, IExplorableModel where TEntity : class, IModel
{

    protected BaseModelEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";


        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected BaseModelEndpointModule(string route) : base(route)
    {

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {


        app.MapGet("", async ([AsParameters] QueryHandler<TExplorer> handler) => await handler.Handle())
            .WithSummary("Using RQL")
            .WithDescription($"Query {typeof(TExplorer).Name.Pluralize()} using RQL")
            .Produces<List<TExplorer>>()
            .Produces<ErrorResponseModel>(400);


        app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("By UID")
            .WithDescription($"Retrieve {typeof(TEntity).Name} by UID")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404);


        app.MapPost("", async ([AsParameters] CreateHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("Create")
            .WithDescription($"Create {typeof(TEntity).Name} from delta RTO")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


        app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("Update")
            .WithDescription($"Update {typeof(TEntity).Name} from delta RTO")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422);


        app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("Delete")
            .WithDescription($"Delete {typeof(TEntity).Name} using UID")
            .Produces(200)
            .Produces<ErrorResponseModel>(404);


        app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("Journal")
            .WithDescription($"{typeof(TEntity).Name} Audit Journal for given UID")
            .Produces<List<AuditJournalModel>>();


        app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
            .WithSummary("Patch")
            .WithDescription($"Apply Patches and Retrieve {typeof(TEntity).Name} but UID")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);


    }


}




