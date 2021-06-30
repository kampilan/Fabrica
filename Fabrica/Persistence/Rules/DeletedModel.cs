using Fabrica.Models.Support;

namespace Fabrica.Persistence.Rules
{


    public interface IDeletedModel
    {
        void SetTarget(object model);
    }


    public sealed class DeletedModel<TModel>: IDeletedModel where TModel : class, IModel
    {

        public TModel Target { get; private set; }

        void IDeletedModel.SetTarget(object model)
        {
            if (model is TModel tm)
                Target = tm;
        }


    }


}
