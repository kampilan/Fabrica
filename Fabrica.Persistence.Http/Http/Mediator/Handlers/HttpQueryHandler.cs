﻿using Fabrica.Http;
using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.Persistence.Http.Mediator.Handlers
{



    public class HttpQueryHandler<TExplorer> : BaseHttpHandler<QueryEntityRequest<TExplorer>, List<TExplorer>> where TExplorer : class, IModel
    {


        public HttpQueryHandler(ICorrelation correlation, IHttpClientFactory factory, IModelMetaService meta) : base(correlation, factory, meta)
        {
        }

        protected override async Task<List<TExplorer>> Perform(CancellationToken cancellationToken = default)
        {

            using var logger = this.EnterMethod();



            logger.Inspect("Explorer Type", typeof(TExplorer).FullName);



            // *****************************************************************
            logger.Debug("Attempting to get Meta for given explorer type");
            var meta = Meta.GetMetaFromType(typeof(TExplorer));

            logger.LogObject(nameof(meta), meta);



            // *****************************************************************
            logger.Debug("Attempting to build request");
            var request = HttpRequestBuilder.Get()
                .ForResource(meta)
                .WithRql(Request.Filters);

            logger.Inspect(nameof(request), request);



            // *****************************************************************
            logger.Debug("Attempting to send request");
            var response = await Send(request, cancellationToken);



            // *****************************************************************
            logger.Debug("Attempting to build list from body");
            var list = response.FromBodyToList<TExplorer>(new ModelExplorerContractResolver());



            // *****************************************************************
            return list!;



        }


    }

}