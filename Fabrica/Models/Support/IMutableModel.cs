using System.Collections.Generic;
using System.ComponentModel;
using Fabrica.Models.Patch.Builder;
using JetBrains.Annotations;

namespace Fabrica.Models.Support
{


    public interface IMutableModel: IModel, IEditableObject
    {


        bool IsReadonly();
        void Readonly();

        bool IsAdded();
        void Added();

        bool IsModified();

        bool IsRemoved();
        void Removed();

        void Undo();
        void Post();


        IDictionary<string, DeltaProperty> GetDelta();


        void NotifyCollectionChanged([NotNull] IAggregateCollection collection, [CanBeNull] IModel member = null);

        void OnCreate();
        void OnModification();


    }


}
