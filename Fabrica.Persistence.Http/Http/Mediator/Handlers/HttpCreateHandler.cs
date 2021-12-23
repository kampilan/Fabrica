
using Fabrica.Http;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;

namespace Fabrica.Persistence.Http.Mediator.Handlers;

public class HttpCreateHandler<TEntity> : BaseHttpHandler<CreateEntityRequest<TEntity>, TEntity> where TEntity : class, IModel
{

    public HttpCreateHandler( ICorrelation correlation, IHttpClientFactory factory, IModelMetaService meta) : base(correlation, factory, meta)
    {
    }

    protected override async Task<TEntity> Perform(CancellationToken cancellationToken = default)
    {


        using var logger = EnterMethod();


        logger.Inspect("Entity Type", typeof(TEntity).FullName);



        // *****************************************************************
        logger.Debug("Attempting to get Meta for given explorer type");
        var meta = Meta.GetMetaFromType(typeof(TEntity));



        // *****************************************************************
        logger.Debug("Attempting to build request");
        var request = HttpRequestBuilder.Post()
            .ForResource(meta.Resource)
            .WithBody(Request.Delta);



        // *****************************************************************
        logger.Debug("Attempting to send request");
        var response = await Send(request, cancellationToken);



        // *****************************************************************
        logger.Debug("Attempting to build entity from body");
        var entity = response.FromBody<TEntity>();



        // *****************************************************************
        return entity;



    }


}