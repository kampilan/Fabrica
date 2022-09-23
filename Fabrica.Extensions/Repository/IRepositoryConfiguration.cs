namespace Fabrica.Utilities.Repository
{


    public interface IRepositoryConfiguration
    {

        string RepositoryContainer { get; }

        string PermanentRoot { get; }
        string TransientRoot { get; }
        string ResourceRoot { get; }

    }


}