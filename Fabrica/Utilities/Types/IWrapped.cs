namespace Fabrica.Utilities.Types
{


    public interface IWrapped
    {


        bool IsNew      { get; }
        bool IsModified { get; }
        bool IsDeleted  { get; }

        object Instance { get; }

    }


}
