using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Fabrica.Models.Support
{


    public interface IAggregateCollection: IEditableObject, INotifyCollectionChanged
    {

        string PropertyName { get; }


        IEnumerable<IModel> Members { get; }
        void AddMember(IModel member);
        void RemoveMember(IModel member);


        bool IsModified();

        void Undo();
        void Post();

        IEnumerable<IModel> DeltaMembers { get; }


    }


}
