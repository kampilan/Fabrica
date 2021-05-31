namespace Fabrica.Models.Support
{


    public interface IAggregateModel: IMutableModel
    {

        void SetParent(object parent);


    }

}
