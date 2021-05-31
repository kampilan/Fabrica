namespace Fabrica.Utilities.Storage
{


    public class LookThroughStorageProvider: BaseLookThroughStorageProvider
    {


        public LookThroughStorageProvider( ILocalStorageProvider local, IRemoteStorageProvider remote )
        {
            Local  = local;
            Remote = remote;
        }


    }


}
