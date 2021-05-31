namespace Fabrica.Utilities.Types
{

    public class InstanceWrapper: IWrapped
    {

        public InstanceWrapper( object instance )
        {
            Instance = instance;
        }


        public bool IsNew { get; set; }
        public bool IsModified { get; set; }
        public bool IsDeleted { get; set; }


        public object Instance { get; }

    }

}