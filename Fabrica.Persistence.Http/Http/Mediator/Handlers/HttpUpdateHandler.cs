
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Http;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;

namespace Fabrica.Persistence.Http.Mediator.Handlers
{


    public class HttpUpdateHandler<TEntity> : BaseHttpHandler<UpdateEntityRequest<TEntity>, TEntity> where TEntity : class, IModel
    {

        public HttpUpdateHandler(ICorrelation correlation, IHttpClientFactory factory, IModelMetaService meta) : base(correlation, factory, meta)
        {
        }

        protected override async Task<TEntity> Perform(CancellationToken cancellationToken = default)
        {


            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect("Entity Type", typeof(TEntity).FullName);



                // *****************************************************************
                logger.Debug("Attempting to get Meta for given explorer type");
                var meta = Meta.GetMetaFromType(typeof(TEntity));

                logger.LogObject(nameof(meta), meta);



                // *****************************************************************
                logger.Debug("Attempting to build request");
                var request = HttpRequestBuilder.Put()
                    .ForResource(meta.Resource)
                    .WithIdentifier(Request.Uid)
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
            finally
            {
                logger.LeaveMethod();
            }


        }


    }

}
