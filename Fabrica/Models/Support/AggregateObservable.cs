using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Fabrica.Watch;

namespace Fabrica.Models.Support
{


    public sealed class AggregateObservable<TMember>: ObservableCollection<TMember>, IAggregateCollection where TMember : class, IAggregateModel, IEditableObject, INotifyPropertyChanged
    {


        public AggregateObservable( IMutableModel owner, string propertyName )
        {
            AttachOwner(owner, propertyName);
        }


        public AggregateObservable( IMutableModel owner, string propertyName, IEnumerable<TMember> members )
        {

            AttachOwner(owner,propertyName);

            foreach (var mem in members)
                Add(mem);

        }

        public IMutableModel Owner { get; private set; }
        public string PropertyName { get; private set; }

        public void AttachOwner( IMutableModel owner, string property )
        {
            Owner        = owner;
            PropertyName = property;
        }

        private void OnMemberPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Owner.NotifyCollectionChanged(this, (IModel)sender);
        }


        private HashSet<TMember> Guard { get; } = new HashSet<TMember>();
        private List<TMember> RemovedList { get; } = new List<TMember>();



        #region Observable overrides

        protected override void InsertItem( int index, TMember item )
        {

            if (item == null || !Guard.Add(item)) 
                return;

            item.SetParent(Owner);
            item.PropertyChanged += OnMemberPropertyChanged;

            base.InsertItem(index, item);

            Owner.NotifyCollectionChanged(this);

        }

        protected override void SetItem( int index, TMember item )
        {

            if( item == null || !Guard.Add(item) )
                return;

            item.SetParent(Owner);
            item.PropertyChanged += OnMemberPropertyChanged;

            base.SetItem(index, item);

            Owner.NotifyCollectionChanged(this);

        }

        protected override void ClearItems()
        {

            foreach (var item in this)
            {

                if( !Guard.Remove(item) ) 
                    continue;

                item.SetParent(null);
                item.PropertyChanged -= OnMemberPropertyChanged;

                if ( item.IsAdded() )
                    continue;

                item.Removed();
                RemovedList.Add(item);

            }

            base.ClearItems();

            Owner.NotifyCollectionChanged(this);


        }

        protected override void RemoveItem( int index )
        {

            using var logger = this.EnterMethod();

            logger.Inspect(nameof(index), index);

            logger.LogObject(nameof(Guard), Guard);

            var item = this[index];
            if (item == null || !Guard.Remove(item))
                return;

            logger.LogObject(nameof(item), item);

            item.SetParent(null);
            item.PropertyChanged -= OnMemberPropertyChanged;

            if ( !item.IsAdded() )
            {
                item.Removed();
                RemovedList.Add(item);
                logger.LogObject(nameof(RemovedList), RemovedList);
            }

            base.RemoveItem(index);

            Owner.NotifyCollectionChanged(this);

        }

        #endregion


        #region IAggregateCollection implementation


        public bool IsModified()
        {
            var modified = Guard.Any(m => m.IsModified() || m.IsAdded()) || RemovedList.Count > 0;
            return modified;
        }

        void IAggregateCollection.AddMember( IModel item )
        {

            if (item is TMember member)
                Add( member );
        }

        void IAggregateCollection.RemoveMember( IModel item )
        {
            if (item is TMember member)
                Remove(member);
        }

        IEnumerable<IModel> IAggregateCollection.Members => Guard;

        IEnumerable<IModel> IAggregateCollection.DeltaMembers => Guard.Where(m => m.IsModified() || m.IsAdded()).Union(RemovedList);

        #endregion


        #region IEditableObject implementation

        public void BeginEdit()
        {
            foreach (var e in Guard)
                e.BeginEdit();
        }

        public void CancelEdit()
        {

            Undo();
        }


        public void EndEdit()
        {
            foreach (var e in Guard)
                e.EndEdit();
        }


        public void Undo()
        {

            var added = new List<TMember>();

            foreach (var mem in Guard)
            {
                if (mem.IsAdded())
                    added.Add(mem);
                else
                    mem.Undo();
            }

            foreach (var mem in added)
            {
                mem.PropertyChanged -= OnMemberPropertyChanged;
                Remove(mem);
            }

            foreach( var mem in RemovedList )
            {

                mem.Undo();

                mem.PropertyChanged += OnMemberPropertyChanged;
                Add( mem );

            }

            RemovedList.Clear();


        }

        public void Post()
        {

            RemovedList.Clear();

            foreach (var mem in Guard)
                mem.Post();

        }

        #endregion


    }


}
