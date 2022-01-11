using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Fabrica.Models.Patch.Builder;

// ReSharper disable UnusedMember.Local

namespace Fabrica.Models.Support
{

    
    
    public abstract class BaseMutableModel<TImp>: BaseModel<TImp>, IMutableModel, IJsonOnDeserializing, IJsonOnDeserialized where TImp: BaseMutableModel<TImp>
    {


        #region State Management


        public void SuspendTracking(Action<TImp> op)
        {

            EnterSuspendTracking();
            try
            {
                op((TImp)this);
            }
            finally
            {
                ExitSuspendTracking();
            }

        }

        public void EnterSuspendTracking()
        {
            _isReadonly = true;
        }

        public void ExitSuspendTracking()
        {
            _isReadonly = false;
        }


        [OnDeserializing]
        private void BeforeDeserializing(StreamingContext context)
        {
            EnterSuspendTracking();
        }

        public void OnDeserializing()
        {
            EnterSuspendTracking();
        }


        [OnDeserialized]
        private void AfterDeserializing(StreamingContext context)
        {
            ExitSuspendTracking();
        }

        public void OnDeserialized()
        {
            ExitSuspendTracking();
        }


        private bool _isReadonly;
        public bool IsReadonly()
        {
            return _isReadonly;
        }
        public void Readonly()
        {
            CancelEdit();
            _isReadonly = true;
            _isAdded = false;
            _isRemoved = false;
        }


        private bool _editing;
        public bool IsEditing() => _editing;


        private bool _isAdded;
        public bool IsAdded()
        {
            return _isAdded;
        }

        public void Added()
        {
            if (!_isReadonly)
                _isAdded = true;
        }


        public bool IsModified()
        {
            return Delta.Count > 0 || IsAdded() || IsRemoved();
        }


        private bool _isRemoved;
        public bool IsRemoved()
        {
            return _isRemoved;
        }

        public void Removed()
        {

            if (_isReadonly)
                return;

            CancelEdit();

            _isAdded = false;
            _isRemoved = true;

        }

        #endregion


        #region Change management


        private IDictionary<string, DeltaProperty> Delta { get; } = new Dictionary<string, DeltaProperty>();

        public IDictionary<string, DeltaProperty> GetDelta()
        {
            return Delta;
        }

        public event PropertyChangedEventHandler PropertyChanged;



        public virtual void NotifyPropertyChanged(string propertyName, object before, object after)
        {


            if (_isReadonly)
                return;


            // ********************************************************
            // Parent. No need to include 
            if (!(after is IReferenceModel) && after is IModel)
              return;



            // ********************************************************
            if (Delta.TryGetValue(propertyName, out var prop))
                prop.Current = after;
            else
            {
                var newProp = new DeltaProperty
                {
                    Name = propertyName,
                    Original = before,
                    Current = after
                };

                if (after is IReferenceModel || before is IReferenceModel)
                {
                    newProp.IsReference = true;
                    newProp.Parent = this.ToAlias();
                    newProp.ParentUid = Uid;
                }

                Delta[propertyName] = newProp;

            }


            // ********************************************************
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        }

        public virtual void NotifyCollectionChanged(IAggregateCollection collection, IModel member = null)
        {


            if (_isReadonly)
                return;


            // ********************************************************
            if (Delta.TryGetValue(collection.PropertyName, out var prop))
                prop.Current = collection;
            else
            {

                var newProp = new DeltaProperty
                {
                    IsCollection = true,
                    Parent = this.ToAlias(),
                    ParentUid = Uid,
                    Name = collection.PropertyName,
                    Original = collection,
                    Current = collection
                };

                Delta[collection.PropertyName] = newProp;

            }


            // ********************************************************
            var handler = PropertyChanged;

            handler?.Invoke(this, new PropertyChangedEventArgs(collection.PropertyName));

        }


        #endregion


        #region Editable members

        public void BeginEdit()
        {
            _editing = true;
        }

        public void CancelEdit()
        {
            _editing = false;
            Undo();
        }


        public void EndEdit()
        {
            _editing = false;
        }

        public void Undo()
        {

            _isRemoved = false;

            foreach (var pair in Delta)
            {

                if (pair.Value.IsCollection && pair.Value.Current is IAggregateCollection collection)
                    collection.Undo();
                else
                {
                    var propInfo = GetType().GetProperty(pair.Key);
                    propInfo?.SetMethod?.Invoke(this, new[] { pair.Value.Original });
                }

            }

            Delta.Clear();


        }

        public void Post()
        {

            foreach (var pair in Delta)
            {
                if (pair.Value.IsCollection && pair.Value.Current is IAggregateCollection collection)
                    collection.Post();
            }

            Delta.Clear();

            _isAdded = false;
            _isRemoved = false;

        }



        #endregion


        #region Lifecycle


        public virtual void OnCreate()
        {
        }

        public virtual void OnModification()
        {
        }

        #endregion


    }



}
