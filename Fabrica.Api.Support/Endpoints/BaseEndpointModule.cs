using System.Reflection;
using Fabrica.Api.Support.Models;
using Fabrica.Models;
using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;

namespace Fabrica.Api.Support.Endpoints;


[AttributeUsage(AttributeTargets.Class)]
public class BasePathAttribute : Attribute
{

    public string Prefix { get; set; } = "";
    private string Resource { get; set; } = "";
    public string Path => $"{Prefix}/{Resource}";

}


public abstract class BaseEndpointModule: AbstractEndpointModule
{

    static BaseEndpointModule()
    {

        Settings = new JsonSerializerSettings
        {
            ContractResolver = new ModelContractResolver(),
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            NullValueHandling = NullValueHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

    }

    public static JsonSerializerSettings Settings { get; }


    protected BaseEndpointModule()
    {

        if (GetType().GetCustomAttribute<BasePathAttribute>() is { } attr)
            BasePath = attr.Path;

    }


    protected BaseEndpointModule( string route )
    {

        BasePath = route;

    }

}


public abstract class BaseQueryEndpointModule<TExplorer,TEntity> : BaseEndpointModule where TExplorer : class, IExplorableModel where TEntity: class, IMutableModel
{


    protected BaseQueryEndpointModule()
    {

        var attr = typeof(TEntity).GetCustomAttribute<ModelAttribute>();
        BasePath = attr is not null ? attr.Resource : typeof(TEntity).Name.ToLowerInvariant().Pluralize();

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

        app.MapGet("", async ([AsParameters] QueryHandler handler) => await handler.Handle())
            .WithSummary("Using RQL")
            .WithDescription($"Query {typeof(TExplorer).Name.Pluralize()} using RQL")
            .Produces<List<TExplorer>>()
            .Produces<ErrorResponseModel>(400);


        app.MapGet("{uid}", async ([AsParameters] RetrieveHandler handler) => await handler.Handle())
            .WithSummary("By UID")
            .WithDescription($"Retrieve {typeof(TEntity).Name} by UID")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404);


        app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler handler) => await handler.Handle())
            .WithSummary("Journal")
            .WithDescription($"{typeof(TEntity).Name} Audit Journal by UID")
            .Produces<List<AuditJournalModel>>();

    }

    protected class QueryHandler : BaseMediatorHandler<QueryEntityRequest<TExplorer>, List<TExplorer>>
    {


        [FromQuery]
        public string Rql { get; set; } = null!;


        [FromQuery]
        public int? Limit { get; set; }

        protected override Task<QueryEntityRequest<TExplorer>> BuildRequest()
        {

            var list = Request.Query["rql"].Where(rql => rql is not null).Cast<string>().ToList();
            

            var request = new QueryEntityRequest<TExplorer>();

            foreach (var tree in list.Select(RqlLanguageParser.ToCriteria))
                request.Filters.Add(new RqlFilterBuilder<TExplorer>(tree) {RowLimit = Limit??0});

            return Task.FromResult(request);

        }


    }


    protected class RetrieveHandler : BaseMediatorHandler<RetrieveEntityRequest<TEntity>,TEntity>
    {


        [FromRoute] 
        public string Uid { get; set; } = null!;


        protected override Task<RetrieveEntityRequest<TEntity>> BuildRequest()
        {

            var request = new RetrieveEntityRequest<TEntity>
            {
                Uid = Uid
            };

            return Task.FromResult(request);

        }


    }


    protected class JournalHandler : BaseMediatorHandler<AuditJournalStreamRequest, MemoryStream>
    {


        [FromRoute]
        public string Uid { get; set; } = null!;


        protected override Task<AuditJournalStreamRequest> BuildRequest()
        {

            var request = new AuditJournalStreamRequest()
            {
                Entity = typeof(TEntity).FullName ?? "",
                EntityUid = Uid,
            };

            return Task.FromResult(request);

        }


    }



}


public abstract class BaseQueryEndpointModule<TCriteria,TExplorer,TEntity> : BaseEndpointModule where TCriteria : BaseCriteria where TExplorer : class, IExplorableModel where TEntity: class, IMutableModel
{

    protected BaseQueryEndpointModule()
    {

        var attr = typeof(TEntity).GetCustomAttribute<ModelAttribute>();
        BasePath = attr is not null ? attr.Resource : typeof(TEntity).Name.ToLowerInvariant().Pluralize();

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


        app.MapGet("", async ([AsParameters] QueryHandler handler) => await handler.Handle())
            .WithSummary("Using RQL")
            .WithDescription($"Query {typeof(TExplorer).Name.Pluralize()} using RQL")
            .Produces<List<TExplorer>>()
            .Produces<ErrorResponseModel>(400);


        app.MapGet("{uid}", async ([AsParameters] RetrieveHandler handler) => await handler.Handle())
            .WithSummary("By UID")
            .WithDescription($"Retrieve {typeof(TEntity).Name} by UID")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404);


        app.MapGet("{uid}/journal", async ([AsParameters] JournalHandler handler) => await handler.Handle())
            .WithSummary("Journal")
            .WithDescription($"{typeof(TEntity).Name} Audit Journal by UID")
            .Produces<List<AuditJournalModel>>();


    }



    protected class QueryHandler : BaseMediatorHandler<QueryEntityRequest<TExplorer>, List<TExplorer>>
    {


        [FromQuery] 
        public int? Limit { get; set; }

        [FromBody]
        public TCriteria Criteria { get; set; } = null!;


        protected override Task<QueryEntityRequest<TExplorer>> BuildRequest()
        {

            using var logger = EnterMethod();


            var request = new QueryEntityRequest<TExplorer>();


            // *****************************************************************
            logger.Debug("Attempting to produce filters from Criteria");
            if( Criteria.Rql?.Any()??false )
            {
                request.Filters.AddRange(Criteria.Rql.Select(s =>
                {
                    var tree = RqlLanguageParser.ToCriteria(s);
                    return new RqlFilterBuilder<TExplorer>(tree) { RowLimit = Limit??0 };
                }));
            }
            else
            {

                // *****************************************************************
                logger.Debug("Attempting to introspect criteria RQL");
                var filter = RqlFilterBuilder<TExplorer>.Create().Introspect(Criteria);
                filter.RowLimit = Limit ?? 0;

                // *****************************************************************
                request.Filters.Add(filter);

            }

            return Task.FromResult(request);

        }


    }

    protected class RetrieveHandler : BaseMediatorHandler<RetrieveEntityRequest<TEntity>, TEntity>
    {



        [FromRoute]
        public string Uid { get; set; } = null!;


        protected override Task<RetrieveEntityRequest<TEntity>> BuildRequest()
        {

            var request = new RetrieveEntityRequest<TEntity>
            {
                Uid = Uid
            };

            return Task.FromResult(request);

        }


    }

    protected class JournalHandler : BaseMediatorHandler<AuditJournalStreamRequest, MemoryStream>
    {


        [FromRoute]
        public string Uid { get; set; } = null!;


        protected override Task<AuditJournalStreamRequest> BuildRequest()
        {

            var request = new AuditJournalStreamRequest()
            {
                Entity = typeof(TEntity).FullName ?? "",
                EntityUid = Uid,
            };

            return Task.FromResult(request);

        }


    }




}


public abstract class BaseCommandEndpointModule<TDelta,TEntity>: BaseEndpointModule where TDelta: class where TEntity: class, IModel
{


    protected BaseCommandEndpointModule()
    {

        var attr = typeof(TEntity).GetCustomAttribute<ModelAttribute>();
        BasePath = attr is not null ? attr.Resource : typeof(TEntity).Name.ToLowerInvariant().Pluralize();


        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected BaseCommandEndpointModule(string route) : base(route)
    {

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapPost("", async ([AsParameters] CreateHandler handler) => await handler.Handle())
            .WithSummary("Create")
            .WithDescription($"Create {typeof(TEntity).Name} from delta RTO")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);

        app.MapPut("{uid}", async ([AsParameters] UpdateHandler handler) => await handler.Handle())
            .WithSummary("Update")
            .WithDescription($"Update {typeof(TEntity).Name} from delta RTO")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(404)
            .Produces<ErrorResponseModel>(422);


        app.MapDelete("{uid}", async ([AsParameters] DeleteHandler handler) => await handler.Handle())
            .WithSummary("Delete")
            .WithDescription($"Delete {typeof(TEntity).Name} using UID")
            .Produces(200)
            .Produces<ErrorResponseModel>(404);

    }



    protected class CreateHandler : BaseMediatorHandler<CreateEntityRequest<TEntity>,TEntity>
    {


        [FromBody] 
        public TDelta Delta { get; set; } = null!;
            

        protected override Task<CreateEntityRequest<TEntity>> BuildRequest()
        {

            using var logger = EnterMethod();
            
            var request = new CreateEntityRequest<TEntity>();
            request.FromObject(Delta);

            return Task.FromResult(request);

        }


    }

    protected class UpdateHandler : BaseMediatorHandler<UpdateEntityRequest<TEntity>, TEntity>
    {


        [FromRoute] 
        public string Uid { get; set; } = null!;


        [FromBody]
        public TDelta Delta { get; set; } = null!;


        protected override Task<UpdateEntityRequest<TEntity>> BuildRequest()
        {

            using var logger = EnterMethod();

            var request = new UpdateEntityRequest<TEntity>
            {
                Uid = Uid
            };

            request.FromObject(Delta);

            return Task.FromResult(request);

        }


    }


    protected class DeleteHandler : BaseMediatorHandler<DeleteEntityRequest<TEntity>>
    {


        [FromRoute]
        public string Uid { get; set; } = null!;


        protected override Task<DeleteEntityRequest<TEntity>> BuildRequest()
        {

            using var logger = EnterMethod();

            var request = new DeleteEntityRequest<TEntity>
            {
                Uid = Uid
            };

            return Task.FromResult(request);

        }


    }


}