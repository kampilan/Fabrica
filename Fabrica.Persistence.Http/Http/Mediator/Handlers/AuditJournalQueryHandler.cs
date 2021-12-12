using Fabrica.Http;
using Fabrica.Models;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;

namespace Fabrica.Persistence.Http.Mediator.Handlers;

public class HttpAuditJournalQueryHandler<TEntity>: BaseHttpHandler<AuditJournalQueryRequest<TEntity>,List<AuditJournalModel>> where TEntity: class, IMutableModel
{


    public HttpAuditJournalQueryHandler( ICorrelation correlation, IHttpClientFactory factory, IModelMetaService meta) : base(correlation, factory, meta)
    {
    }


    protected override async Task<List<AuditJournalModel>> Perform(CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();


        logger.Inspect("Entity Type", typeof(TEntity).FullName);



        // *****************************************************************
        logger.Debug("Attempting to get Meta for given explorer type");
        var meta = Meta.GetMetaFromType(typeof(TEntity));



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = HttpRequestBuilder.Get()
            .ForResource(meta.Resource)
            .WithIdentifier(Request.Uid)
            .WithSubResource("journal");



        // *****************************************************************
        logger.Debug("Attempting to send request");
        var response = await SendAsync(request,cancellationToken);



        // *****************************************************************
        logger.Debug("Attempting to build entity from body");
        var list = response.FromBodyToList<AuditJournalModel>();



        // *****************************************************************
        return list;


    }

}