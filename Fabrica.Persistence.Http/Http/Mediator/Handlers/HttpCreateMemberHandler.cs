using Fabrica.Http;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Utilities.Container;

namespace Fabrica.Persistence.Http.Mediator.Handlers
{



    public class HttpCreateMemberHandler<TParent, TMember> : BaseHttpHandler<CreateMemberEntityRequest<TParent, TMember>, TMember> where TParent : class, IModel where TMember : class, IModel
    {

        public HttpCreateMemberHandler(ICorrelation correlation, IHttpClientFactory factory, IModelMetaService meta) : base(correlation, factory, meta)
        {
        }

        protected override async Task<TMember> Perform(CancellationToken cancellationToken = default)
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect("Parent Type", typeof(TMember).FullName);
                logger.Inspect("Member Type", typeof(TMember).FullName);



                // *****************************************************************
                logger.Debug("Attempting to get Meta for given explorer type");
                var pm = Meta.GetMetaFromType(typeof(TParent));
                var mm = Meta.GetMetaFromType(typeof(TMember));



                // *****************************************************************
                logger.Debug("Attempting to build request");
                var request = HttpRequestBuilder.Post()
                    .ForResource(pm.Resource)
                    .WithIdentifier(Request.ParentUid)
                    .WithSubResource(mm.Resource)
                    .WithBody(Request.Delta);



                // *****************************************************************
                logger.Debug("Attempting to send request");
                var response = await Send(request, cancellationToken);



                // *****************************************************************
                logger.Debug("Attempting to build entity from body");
                var entity = response.FromBody<TMember>();



                // *****************************************************************
                return entity!;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


    }

}