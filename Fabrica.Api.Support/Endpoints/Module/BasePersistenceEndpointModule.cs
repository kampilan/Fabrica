﻿using Fabrica.Mediator;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.Patch;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Api.Support.Endpoints.Module;

public abstract class BasePersistenceEndpointModule : BaseEndpointModule
{

    protected BasePersistenceEndpointModule()
    {

    }

    protected BasePersistenceEndpointModule(string route) : base(route)
    {

    }

    protected class QueryHandler<TExplorer> : BaseMediatorEndpointHandler<QueryEntityRequest<TExplorer>, List<TExplorer>> where TExplorer : class, IExplorableModel
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
                request.Filters.Add(new RqlFilterBuilder<TExplorer>(tree) { RowLimit = Limit ?? 0 });

            return Task.FromResult(request);

        }


    }


    protected class QueryHandler<TCriteria, TExplorer> : BaseMediatorEndpointHandler<QueryEntityRequest<TExplorer>, List<TExplorer>> where TCriteria : BaseCriteria where TExplorer : class, IExplorableModel
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
            logger.Debug("Attempting to introspect criteria RQL");
            var criteria = RqlFilterBuilder<TExplorer>.Create().Introspect(Criteria);
            criteria.RowLimit = Limit ?? 0;

            // *****************************************************************
            request.Filters.Add(criteria);



            // *****************************************************************
            logger.Debug("Attempting to produce filters from additional RQL");
            if( Criteria.Rql?.Any() ?? false )
            {
                request.Filters.AddRange(Criteria.Rql.Select(s =>
                {
                    var tree = RqlLanguageParser.ToCriteria(s);
                    return new RqlFilterBuilder<TExplorer>(tree);
                }));
            }


            return Task.FromResult(request);


        }


    }


    protected class ThinQueryHandler<TExplorer> : BaseMediatorEndpointHandler<QueryThinEntityRequest<TExplorer>, MemoryStream> where TExplorer : class, IExplorableModel
    {


        [FromQuery]
        public string Rql { get; set; } = null!;


        [FromQuery]
        public int? Limit { get; set; }

        protected override Task<QueryThinEntityRequest<TExplorer>> BuildRequest()
        {

            var list = Request.Query["rql"].Where(rql => rql is not null).Cast<string>().ToList();


            var request = new QueryThinEntityRequest<TExplorer>();

            foreach (var tree in list.Select(RqlLanguageParser.ToCriteria))
                request.Filters.Add(new RqlFilterBuilder<TExplorer>(tree) { RowLimit = Limit ?? 0 });

            return Task.FromResult(request);

        }


    }


    protected class ThinQueryHandler<TCriteria, TExplorer> : BaseMediatorEndpointHandler<QueryThinEntityRequest<TExplorer>, MemoryStream> where TCriteria : BaseCriteria where TExplorer : class, IExplorableModel
    {


        [FromQuery]
        public int? Limit { get; set; }

        [FromBody]
        public TCriteria Criteria { get; set; } = null!;


        protected override Task<QueryThinEntityRequest<TExplorer>> BuildRequest()
        {

            using var logger = EnterMethod();


            var request = new QueryThinEntityRequest<TExplorer>();


            // *****************************************************************
            logger.Debug("Attempting to introspect criteria RQL");
            var criteria = RqlFilterBuilder<TExplorer>.Create().Introspect(Criteria);
            criteria.RowLimit = Limit ?? 0;

            // *****************************************************************
            request.Filters.Add(criteria);



            // *****************************************************************
            logger.Debug("Attempting to produce filters from additional RQL");
            if (Criteria.Rql?.Any() ?? false)
            {
                request.Filters.AddRange(Criteria.Rql.Select(s =>
                {
                    var tree = RqlLanguageParser.ToCriteria(s);
                    return new RqlFilterBuilder<TExplorer>(tree);
                }));
            }


            return Task.FromResult(request);


        }


    }


    protected class RetrieveHandler<TEntity> : BaseMediatorEndpointHandler<RetrieveEntityRequest<TEntity>, TEntity> where TEntity : class, IModel
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



    protected class CreateHandler<TDelta,TEntity> : BaseMediatorEndpointHandler<CreateEntityRequest<TEntity>, TEntity> where TDelta : BaseDelta where TEntity : class, IModel
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

    protected class CreateMemberHandler<TParent,TDelta, TEntity> : BaseMediatorEndpointHandler<CreateMemberEntityRequest<TParent,TEntity>, TEntity> where TParent: class, IModel where TDelta : BaseDelta where TEntity : class, IAggregateModel
    {


        [FromQuery]
        public string Uid { get; set; } = null!;

        [FromBody]
        public TDelta Delta { get; set; } = null!;


        protected override Task<CreateMemberEntityRequest<TParent,TEntity>> BuildRequest()
        {

            using var logger = EnterMethod();

            var request = new CreateMemberEntityRequest<TParent,TEntity>
            {
                ParentUid = Uid
            };

            request.FromObject(Delta);

            return Task.FromResult(request);

        }


    }



    protected class CreateHandler<TEntity> : BaseMediatorEndpointHandler<CreateEntityRequest<TEntity>, TEntity> where TEntity : class, IModel
    {


        [FromBody]
        public Dictionary<string, object> Delta { get; set; } = null!;


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


    protected class CreateMemberHandler<TParent,TEntity> : BaseMediatorEndpointHandler<CreateMemberEntityRequest<TParent,TEntity>, TEntity> where TParent : class, IModel where TEntity : class, IAggregateModel
    {


        [FromQuery]
        public string Uid { get; set; } = null!;

        [FromBody]
        public Dictionary<string, object> Delta { get; set; } = null!;


        protected override Task<CreateMemberEntityRequest<TParent, TEntity>> BuildRequest()
        {

            using var logger = EnterMethod();

            var request = new CreateMemberEntityRequest<TParent, TEntity>
            {
                ParentUid = Uid,
                Delta = Delta
            };

            return Task.FromResult(request);

        }


    }


    protected class UpdateHandler<TDelta, TEntity> : BaseMediatorEndpointHandler<UpdateEntityRequest<TEntity>, TEntity> where TDelta : class where TEntity : class, IModel
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

    protected class UpdateHandler<TEntity> : BaseMediatorEndpointHandler<UpdateEntityRequest<TEntity>, TEntity> where TEntity : class, IModel
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

    protected class DeleteHandler<TEntity> : BaseMediatorEndpointHandler<DeleteEntityRequest<TEntity>> where TEntity : class, IModel
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


    protected class JournalHandler<TEntity> : BaseMediatorEndpointHandler<AuditJournalStreamRequest, MemoryStream> where TEntity : class, IModel
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



    protected class PatchHandler<TEntity> : BaseMediatorEndpointHandler where TEntity : class, IModel
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

            var result = EndpointResult.Create(retrieveResponse);



            // *****************************************************************
            return result;

        }


    }



}