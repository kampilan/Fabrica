using System.Collections.Generic;
using System.Threading.Tasks;
using Fabrica.Mediator;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Fabrica.Watch;

namespace Fabrica.Persistence.Mediator
{


    public static class MediatorExtensions
    {

        public static async Task<List<TExplorer>> Query<TExplorer>(this IMessageMediator mediator, params IRqlFilter<TExplorer>[] filters) where TExplorer : class, IExplorableModel
        {

            var logger = mediator.GetLogger();

            try
            {

                logger.EnterMethod();

                var request = new QueryEntityRequest<TExplorer>();
                request.Filters.AddRange(filters);

                var response = await mediator.Send(request);

                logger.LogObject(nameof(response), response);

                response.EnsureSuccess();

                return response.Value;


            }
            finally
            {
                logger.LeaveMethod();
            }



        }

        public static async Task<List<TExplorer>> Query<TExplorer>(this IMessageMediator mediator, params ICriteria[] criteria) where TExplorer : class, IExplorableModel
        {

            var logger = mediator.GetLogger();

            try
            {

                logger.EnterMethod();


                var request = new QueryEntityRequest<TExplorer>();
                foreach (var c in criteria)
                    request.AddCriteria(c);

                var response = await mediator.Send(request);

                response.EnsureSuccess();

                return response.Value;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public static async Task<TModel> Retrieve<TModel>(this IMessageMediator mediator, string uid) where TModel : class, IModel
        {

            var logger = mediator.GetLogger();

            try
            {

                logger.EnterMethod();


                var request = new RetrieveEntityRequest<TModel> { Uid = uid };

                var response = await mediator.Send(request);

                response.EnsureSuccess();

                return response.Value;



            }
            finally
            {
                logger.LeaveMethod();
            }



        }


        public static async Task<TModel> Apply<TModel>(this IMessageMediator mediator, TModel source) where TModel : class, IMutableModel
        {


            var logger = mediator.GetLogger();

            try
            {

                logger.EnterMethod();

                var request = new PatchEntityRequest<TModel> { Uid = source.Uid };
                request.FromModel(source);

                var response = await mediator.Send(request);

                response.EnsureSuccess();

                return response.Value;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        public static async Task<TModel> Apply<TModel>(this IMessageMediator mediator, string uid, PatchSet set ) where TModel : class, IMutableModel
        {


            var logger = mediator.GetLogger();

            try
            {

                logger.EnterMethod();


                var request = new PatchEntityRequest<TModel> { Uid = uid };
                request.Patches.Add(set);

                var response = await mediator.Send(request);

                response.EnsureSuccess();

                return response.Value;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }




    }


}