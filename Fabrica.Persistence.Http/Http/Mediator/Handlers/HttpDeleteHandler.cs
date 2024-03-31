using Fabrica.Http;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.Persistence.Http.Mediator.Handlers;

public class HttpDeleteHandler<TEntity>(ICorrelation correlation, IHttpClientFactory factory, IModelMetaService meta) : BaseHttpHandler<DeleteEntityRequest<TEntity>>(correlation, factory, meta) where TEntity : class, IModel
{


    protected override async Task Perform(CancellationToken cancellationToken = default)
    {


        using var logger = EnterMethod();


        logger.Inspect("Entity Type", typeof(TEntity).FullName);



        // *****************************************************************
        logger.Debug("Attempting to get Meta for given explorer type");
        var meta = Meta.GetMetaFromType(typeof(TEntity));

        logger.LogObject(nameof(meta), meta);



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = HttpRequestBuilder.Delete()
            .ForResource(meta.Resource)
            .WithIdentifier(Request.Uid);



        // *****************************************************************
        logger.Debug("Attempting to send request");
        await Send(request, cancellationToken);


    }


}