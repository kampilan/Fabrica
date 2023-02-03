using System.Reflection;
using Fabrica.Api.Support.Models;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.Patch;
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
public class ModuleRouteAttribute : Attribute
{

    public string Prefix { get; set; } = "";
    public string Resource { get; set; } = "";
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

    protected static string ExtractResource<T>() where T : class
    {

        var attr = typeof(T).GetCustomAttribute<ModelAttribute>();
        var path = attr is not null ? attr.Resource : "";

        if( string.IsNullOrWhiteSpace(path) )
            path = typeof(T).Name.Pluralize().ToLowerInvariant();

        return path;

    }


    protected BaseEndpointModule()
    {

        if (GetType().GetCustomAttribute<ModuleRouteAttribute>() is { } attr)
            BasePath = attr.Path;

    }


    protected BaseEndpointModule( string route )
    {

        BasePath = route;

    }


    protected class PingHandler: BaseEndpointHandler
    {


        public override Task<IResult> Handle()
        {

            using var logger = EnterMethod();


            var utc = DateTime.UtcNow;

            DateTime Convert(int diff)
            {

                var dt = utc.AddHours(diff);

                return dt;

            }

            var dict = new Dictionary<string, DateTime>
            {
                ["Beverly Hills"] = Convert(-7),
                ["Monte Carlo"] = Convert(2),
                ["London"] = Convert(1),
                ["Paris"] = Convert(2),
                ["Rome"] = Convert(2),
                ["Gstaad"] = Convert(2)
            };


            var response = new Response<Dictionary<string, DateTime>>(dict)
                .IsOk()
                .WithDetail(new EventDetail
                {
                    Category = EventDetail.EventCategory.Info,
                    Explanation = "Time according to the Rouchefoucauld",
                    Group = "Tests",
                    Source = "Louis Winthorpe III"
                });


            var result = BuildResult(response);

            return Task.FromResult(result);

        }

    }



}


public abstract class BaseQueryEndpointModule<TExplorer,TEntity> : BaseEndpointModule where TExplorer : class, IExplorableModel where TEntity: class, IMutableModel
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

    protected class QueryHandler : BaseMediatorEndpointHandler<QueryEntityRequest<TExplorer>, List<TExplorer>>
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


    protected class RetrieveHandler : BaseMediatorEndpointHandler<RetrieveEntityRequest<TEntity>,TEntity>
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


    protected class JournalHandler : BaseMediatorEndpointHandler<AuditJournalStreamRequest, MemoryStream>
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



    protected class QueryHandler : BaseMediatorEndpointHandler<QueryEntityRequest<TExplorer>, List<TExplorer>>
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

    protected class RetrieveHandler : BaseMediatorEndpointHandler<RetrieveEntityRequest<TEntity>, TEntity>
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

    protected class JournalHandler : BaseMediatorEndpointHandler<AuditJournalStreamRequest, MemoryStream>
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

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";


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



    protected class CreateHandler : BaseMediatorEndpointHandler<CreateEntityRequest<TEntity>,TEntity>
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

    protected class UpdateHandler : BaseMediatorEndpointHandler<UpdateEntityRequest<TEntity>, TEntity>
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


    protected class DeleteHandler : BaseMediatorEndpointHandler<DeleteEntityRequest<TEntity>>
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


public abstract class BaseCommandEndpointModule<TEntity> : BaseEndpointModule where TEntity : class, IModel
{


    protected BaseCommandEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";


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



    protected class CreateHandler : BaseMediatorEndpointHandler<CreateEntityRequest<TEntity>, TEntity>
    {


        [FromBody]
        public Dictionary<string,object> Delta { get; set; } = null!;


        protected override Task<CreateEntityRequest<TEntity>> BuildRequest()
        {

            using var logger = EnterMethod();

            var request = new CreateEntityRequest<TEntity>
            {
                Delta = Delta
            };

            return Task.FromResult(request);

        }


    }

    protected class UpdateHandler : BaseMediatorEndpointHandler<UpdateEntityRequest<TEntity>, TEntity>
    {


        [FromRoute]
        public string Uid { get; set; } = null!;


        [FromBody]
        public Dictionary<string, object> Delta { get; set; } = null!;


        protected override Task<UpdateEntityRequest<TEntity>> BuildRequest()
        {

            using var logger = EnterMethod();

            var request = new UpdateEntityRequest<TEntity>
            {
                Uid = Uid,
                Delta = Delta
            };


            return Task.FromResult(request);

        }


    }


    protected class DeleteHandler : BaseMediatorEndpointHandler<DeleteEntityRequest<TEntity>>
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


public abstract class BasePatchEndpointModule<TEntity>: BaseEndpointModule where TEntity : class, IModel
{


    protected BasePatchEndpointModule()
    {

        var attr = GetType().GetCustomAttribute<ModuleRouteAttribute>();
        var prefix = attr is not null ? attr.Prefix : "";
        var resource = !string.IsNullOrWhiteSpace(attr?.Resource) ? attr.Resource : ExtractResource<TEntity>();

        BasePath = $"{prefix}/{resource}";


        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }

    protected BasePatchEndpointModule(string route) : base(route)
    {

        IncludeInOpenApi();
        WithGroupName($"{typeof(TEntity).Name.Pluralize()}");

    }


    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        app.MapPatch("{uid}", async ([AsParameters] PatchHandler handler) => await handler.Handle())
            .WithSummary("Patch")
            .WithDescription($"CreateApply Patches and Retrieve {typeof(TEntity).Name} but UID")
            .Produces<TEntity>()
            .Produces<ErrorResponseModel>(422);

    }


    protected class PatchHandler : BaseMediatorEndpointHandler
    {


        [FromRoute]
        public string Uid { get; set; } = null!;

        [FromServices] 
        public IModelMetaService Meta { get; set; } = null!;

        [FromServices] 
        public IPatchResolver Resolver { get; set; } = null!;


        public override async Task<IResult> Handle()
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to deserialize ModelPatch list");
            var patches = await FromBody<List<ModelPatch>>();

            logger.Inspect(nameof(patches.Count), patches.Count);



            // *****************************************************************
            logger.Debug("Attempting to build patch set");
            var set = new PatchSet();
            set.Add(patches);

            logger.Inspect(nameof(set.Count), set.Count);



            // *****************************************************************
            logger.Debug("Attempting to resolve patch set");
            var requests = Resolver.Resolve(set);



            // *****************************************************************
            logger.Debug("Attempting to send request via Mediator");
            var patchResponse = await Mediator.Send(requests);

            logger.Inspect(nameof(patchResponse.HasErrors), patchResponse.HasErrors);



            // *****************************************************************
            logger.Debug("Attempting to BatchResponse for success");
            patchResponse.EnsureSuccess();



            // *****************************************************************
            logger.Debug("Attempting to retrieve entity using Uid");
            var request = new RetrieveEntityRequest<TEntity>
            {
                Uid = Uid
            };



            // *****************************************************************
            logger.Debug("Attempting to send request via Mediator");

            var retrieveResponse = await Mediator.Send(request);



            // *****************************************************************
            logger.Debug("Attempting to build action result");

            var result = BuildResult(retrieveResponse);



            // *****************************************************************
            return result;

        }


    }




}