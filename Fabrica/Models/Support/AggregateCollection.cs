using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Fabrica.Models.Support
{


    public class AggregateCollection<TMember>: IList<TMember>, IAggregateCollection where TMember: class, IAggregateModel, IEditableObject, INotifyPropertyChanged
    {


        public AggregateCollection( IMutableModel owner, string propertyName )
        {

            Owner        = owner;
            PropertyName = propertyName;

        }


        public AggregateCollection( IMutableModel owner, string propertyName, IEnumerable<TMember> members )
        {

            Owner        = owner;
            PropertyName = propertyName;

            foreach( var mem in members )
                InternalAdd( mem, i=>i.Add(mem));

        }

        public IMutableModel Owner { get; }
        public string PropertyName { get; }


        private void OnMemberPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            Owner.NotifyCollectionChanged( this, (IMutableModel)sender );
        }


        protected HashSet<TMember> Guard { get; } = new HashSet<TMember>();
        protected List<TMember> InnerList { get; } = new List<TMember>();
        protected List<TMember> RemovedList { get; } = new List<TMember>();


        protected void InternalAdd(TMember item, Action<List<TMember>> adder)
        {

            if( item != null && Guard.Add(item) )
            {

                item.SetParent(Owner);
                item.PropertyChanged += OnMemberPropertyChanged;

                adder(InnerList);

                Owner.NotifyCollectionChanged(this);
                FireCollectionChangedAdd(item);

            }

        }

        protected void InternalReplace( int index, TMember item, Action<List<TMember>> replacer )
        {

            var oldItem = InnerList[index];
            if (oldItem != null && Guard.Remove(oldItem))
            {

                item.SetParent(null);

                if( !item.IsAdded() )
                {
                    item.Removed();
                    RemovedList.Add(item);
                }

            }

            if( item != null && Guard.Add(item))
            {

                item.SetParent(Owner);
                item.PropertyChanged += OnMemberPropertyChanged;

                replacer(InnerList);

                Owner.NotifyCollectionChanged(this);
                FireCollectionChangedAdd(item);

            }

        }

        protected bool InternalRemove(TMember item, Action<List<TMember>> remover)
        {

            if (item != null && Guard.Remove(item))
            {

                item.SetParent(null);

                if (!item.IsAdded())
                {
                    item.Removed();
                    RemovedList.Add(item);
                }

                remover(InnerList);

                Owner.NotifyCollectionChanged(this);
                FireCollectionChangedRemove(item);

                return true;

            }

            return false;

        }


        public bool IsModified()
        {
            var modified = InnerList.Any(m => m.IsModified() || m.IsAdded()) || RemovedList.Count > 0;
            return modified;
        }



        #region IAggregateCollection implementation

        void IAggregateCollection.AddMember( IModel model )
        {
            if( model is TMember member )
                InternalAdd( member, i=>i.Add(member) );
        }

        void IAggregateCollection.RemoveMember( IModel model )
        {
            if( model is TMember member )
                InternalRemove(member, i=>i.Remove(member));
        }

        IEnumerable<IModel> IAggregateCollection.Members => InnerList;

        IEnumerable<IModel> IAggregateCollection.DeltaMembers => InnerList.Where(m=>m.IsModified() || m.IsAdded()).Union(RemovedList);

        #endregion


        #region IEditableObject implementation

        public void BeginEdit()
        {
            foreach (var e in InnerList )
                e.BeginEdit();
        }

        public void CancelEdit()
        {

            Undo();
        }


        public void EndEdit()
        {
            foreach( var e in InnerList )
                e.EndEdit();
        }


        public void Undo()
        {

            var added = new List<TMember>();

            foreach (var mem in InnerList)
            {
                if (mem.IsAdded())
                    added.Add(mem);
                else
                    mem.Undo();
            }

            foreach (var mem in added)
            {
                mem.PropertyChanged -= OnMemberPropertyChanged;
                Guard.Remove(mem);
                InnerList.Remove(mem);
            }

            foreach (var mem in RemovedList)
            {

                mem.Undo();

                mem.PropertyChanged += OnMemberPropertyChanged;

                Guard.Add(mem);
                InnerList.Add(mem);
            }

            RemovedList.Clear();


        }

        public void Post()
        {

            RemovedList.Clear();

            foreach ( var mem in InnerList )
            {
                mem.Post();
            }


        }

        #endregion


        #region IList<TMember> Implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)InnerList).GetEnumerator();
        }

        IEnumerator<TMember> IEnumerable<TMember>.GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        void ICollection<TMember>.Add(TMember item)
        {
            InternalAdd(item, i => i.Add(item));
        }

        bool ICollection<TMember>.Remove(TMember item)
        {
            return InternalRemove(item, i => i.Remove(item));
        }

        void ICollection<TMember>.Clear()
        {
            foreach (var mem in InnerList)
                InternalRemove(mem, i => i.Remove(mem));
        }

        bool ICollection<TMember>.Contains(TMember item)
        {
            return Guard.Contains(item);
        }

        void ICollection<TMember>.CopyTo(TMember[] array, int arrayIndex)
        {
            InnerList.CopyTo(array, arrayIndex);
        }

        int ICollection<TMember>.Count => InnerList.Count;

        bool ICollection<TMember>.IsReadOnly => false;

        int IList<TMember>.IndexOf(TMember item)
        {
            return InnerList.IndexOf(item);
        }

        void IList<TMember>.Insert(int index, TMember item)
        {
            InternalAdd( item, i=> i.Insert(index, item) );
        }

        void IList<TMember>.RemoveAt(int index)
        {
            var item = InnerList[index];
            InternalRemove( item, i=>i.RemoveAt(index) );
        }

        TMember IList<TMember>.this[int index]
        {
            get => InnerList[index];
            set => InternalReplace( index, value, i => i[index]= value );
        }


        #endregion


        #region INotifyCollectionChanged implementation

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void FireCollectionChangedAdd( IModel item )
        {

            if( CollectionChanged != null )
            {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item);
                CollectionChanged.Invoke(this, args );
            }

        }

        protected void FireCollectionChangedRemove(IModel item)
        {

            if (CollectionChanged != null)
            {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item);
                CollectionChanged.Invoke(this, args);
            }

        }

        protected void FireCollectionChangedReplace(IModel oldItem, IModel newItem )
        {

            if( CollectionChanged != null )
            {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem);
                CollectionChanged.Invoke(this, args);
            }

        }

        protected void FireCollectionChangedClear(IModel item)
        {

            if (CollectionChanged != null)
            {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item);
                CollectionChanged.Invoke(this, args);
            }

        }

        #endregion


    }


}
