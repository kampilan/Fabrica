
// ReSharper disable UnusedMember.Global

using Fabrica.Models.Support;
using Fabrica.Rql;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Reflection;
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


        if (IncludeQueryEndpoint)
        {
            app.MapGet("", async ([AsParameters] QueryHandler<TExplorer> handler) => await handler.Handle())
                .AddMetaData<List<TExplorer>>(typeof(TEntity).Name.Pluralize(), "Using Criteria", $"Query {typeof(TEntity).Name.Pluralize()} using Criteria");
        }

        if (IncludeRetrieveEndpoint)
        {
            app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "By UID", $"Retrieve {typeof(TEntity).Name} by UID");
        }

        if (IncludeCreateEndpoint)
        {
            app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "Create", $"Create {typeof(TEntity).Name} from delta RTO");
        }

        if (IncludeUpdateEndpoint)
        {
            app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "Update", $"Update {typeof(TEntity).Name} from delta RTO");
        }

        if (IncludeDeleteEndpoint)
        {
            app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData(typeof(TEntity).Name.Pluralize(), "Delete", $"Delete {typeof(TEntity).Name} by UID");
        }

        if (IncludeJournalEndpoint)
        {
            app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<List<AuditJournalModel>>(typeof(TEntity).Name.Pluralize(), "Journal", $"{typeof(TEntity).Name} Audit Journal for given UID");
        }

        if (IncludePatchEndpoint)
        {
            app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "Patch", $"Apply Patches and Retrieve {typeof(TEntity).Name} by UID");
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


        if (IncludeQueryEndpoint)
        {
            app.MapGet("", async ([AsParameters] QueryHandler<TExplorer> handler) => await handler.Handle())
                .AddMetaData<List<TExplorer>>(typeof(TEntity).Name.Pluralize(), "Using RQL", $"Query {typeof(TEntity).Name.Pluralize()} using RQL");
        }

        if (IncludeRetrieveEndpoint)
        {
            app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "By UID", $"Retrieve {typeof(TEntity).Name} by UID");
        }

        if (IncludeCreateEndpoint)
        {
            app.MapPost("", async ([AsParameters] CreateHandler<TDelta, TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "Create", $"Create {typeof(TEntity).Name} from delta RTO");

        }

        if (IncludeUpdateEndpoint)
        {
            app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TDelta, TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "Update", $"Update {typeof(TEntity).Name} from delta RTO");
        }

        if (IncludeDeleteEndpoint)
        {
            app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData(typeof(TEntity).Name.Pluralize(), "Delete", $"Delete {typeof(TEntity).Name} by UID");
        }

        if (IncludeJournalEndpoint)
        {
            app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<List<AuditJournalModel>>(typeof(TEntity).Name.Pluralize(), "Journal", $"{typeof(TEntity).Name} Audit Journal for given UID");
        }

        if (IncludePatchEndpoint)
        {
            app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "Patch", $"Apply Patches and Retrieve {typeof(TEntity).Name} by UID");
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


        if (IncludeQueryEndpoint)
        {
            app.MapGet("", async ([AsParameters] QueryHandler<TExplorer> handler) => await handler.Handle())
                .AddMetaData<List<TExplorer>>(typeof(TEntity).Name.Pluralize(), "Using RQL", $"Query {typeof(TEntity).Name.Pluralize()} using RQL");
        }

        if (IncludeRetrieveEndpoint)
        {
            app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "By UID", $"Retrieve {typeof(TEntity).Name} by UID");
        }

        if (IncludeCreateEndpoint)
        {
            app.MapPost("", async ([AsParameters] CreateHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "Create", $"Create {typeof(TEntity).Name} from delta");

        }

        if (IncludeUpdateEndpoint)
        {
            app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "Update", $"Update {typeof(TEntity).Name} from delta");
        }

        if (IncludeDeleteEndpoint)
        {
            app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData(typeof(TEntity).Name.Pluralize(), "Delete", $"Delete {typeof(TEntity).Name} by UID");
        }

        if (IncludeJournalEndpoint)
        {
            app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<List<AuditJournalModel>>(typeof(TEntity).Name.Pluralize(), "Journal", $"{typeof(TEntity).Name} Audit Journal for given UID");
        }

        if (IncludePatchEndpoint)
        {
            app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "Patch", $"Apply Patches and Retrieve {typeof(TEntity).Name} by UID");
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


        if (IncludeQueryEndpoint)
        {
            app.MapGet("", async ([AsParameters] QueryHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<List<TEntity>>(typeof(TEntity).Name.Pluralize(), "Using RQL", $"Query {typeof(TEntity).Name.Pluralize()} using RQL");
        }

        if (IncludeRetrieveEndpoint)
        {
            app.MapGet("{uid}", async ([AsParameters] RetrieveHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "By UID", $"Retrieve {typeof(TEntity).Name} by UID");
        }

        if (IncludeCreateEndpoint)
        {
            app.MapPost("", async ([AsParameters] CreateHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "Create", $"Create {typeof(TEntity).Name} from delta");

        }

        if (IncludeUpdateEndpoint)
        {
            app.MapPut("{uid}", async ([AsParameters] UpdateHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "Update", $"Update {typeof(TEntity).Name} from delta");
        }

        if (IncludeDeleteEndpoint)
        {
            app.MapDelete("{uid}", async ([AsParameters] DeleteHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData(typeof(TEntity).Name.Pluralize(), "Delete", $"Delete {typeof(TEntity).Name} by UID");
        }

        if (IncludeJournalEndpoint)
        {
            app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<List<AuditJournalModel>>(typeof(TEntity).Name.Pluralize(), "Journal", $"{typeof(TEntity).Name} Audit Journal for given UID");
        }

        if (IncludePatchEndpoint)
        {
            app.MapPatch("{uid}", async ([AsParameters] PatchHandler<TEntity> handler) => await handler.Handle())
                .AddMetaData<TEntity>(typeof(TEntity).Name.Pluralize(), "Patch", $"Apply Patches and Retrieve {typeof(TEntity).Name} by UID");
        }


    }


}




