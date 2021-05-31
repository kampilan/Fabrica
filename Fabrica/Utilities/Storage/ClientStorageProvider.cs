namespace Fabrica.Utilities.Storage
{


    public class ClientStorageProvider: BaseLookThroughStorageProvider
    {


        public ClientStorageProvider( ILocalStorageProvider local, IRemoteStorageProvider remote)
        {

            Local = local;

            Remote = new BackgroudStorageProvider(remote);

        }


    }


}
