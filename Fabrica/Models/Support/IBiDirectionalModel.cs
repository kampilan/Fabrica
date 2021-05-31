namespace Fabrica.Models.Support
{


    public interface IBiDirectionalModel
    {
        void SetParent(object parent);

    }


    public interface IBiDirectionalModel<TParent>: IBiDirectionalModel where TParent: class, IModel
    {
        TParent Parent { get; set; }

    }





}
