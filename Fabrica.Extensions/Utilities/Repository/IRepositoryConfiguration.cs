namespace Fabrica.Utilities.Repository
{


    public interface IRepositoryConfiguration
    {

        string PermanentContainer { get; }
        string PermanentRoot { get; }


        string TransientContainer { get; }
        string TransientRoot { get; }


        string ResourceContainer { get; }
        string ResourceRoot { get; }


    }


}