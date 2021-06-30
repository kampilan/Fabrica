using Fabrica.Models.Support;

namespace Fabrica.Persistence.Rules
{

    public interface IUpdatedModel
    {
        void SetTarget(object model);
    }


    public sealed class UpdatedModel<TModel>: IUpdatedModel where TModel : class, IModel
    {

        public TModel Target { get; private set; }

        public void SetTarget(object model)
        {
            if (model is TModel tm)
                Target = tm;
        }


    }


}
