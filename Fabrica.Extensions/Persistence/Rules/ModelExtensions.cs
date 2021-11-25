using Fabrica.Models.Support;

namespace Fabrica.Persistence.Rules
{

    
    public static class ModelExtensions
    {


        public static ICreatedModel ForCreate<TModel>( this TModel target ) where TModel : class, IMutableModel
        {

            ICreatedModel cm = new CreatedModel<TModel>();

            cm.SetTarget(target);

            return cm;

        }


        public static IUpdatedModel ForUpdate<TModel>( this TModel target ) where TModel : class, IMutableModel
        {

            IUpdatedModel um = new UpdatedModel<TModel>();

            um.SetTarget(target);

            return um;

        }

        public static IDeletedModel ForDeleted<TModel>( this TModel target ) where TModel : class, IMutableModel
        {

            IDeletedModel dm = new DeletedModel<TModel>();

            dm.SetTarget(target);

            return dm;

        }



    }


}
