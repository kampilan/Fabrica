/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using JetBrains.Annotations;

namespace Fabrica.Utilities.Types
{

    public class ExpandoWrapper<TTarget> : DynamicObject
    {

        public ExpandoWrapper([NotNull] TTarget target )
        {

            if (target == null) throw new ArgumentNullException(nameof(target));

            Target = target;
        }

        
        public TTarget Target { get;  }

        public bool Tracked { get; set; } = true;

        public Func<TTarget, string, object> MissingPropertyHandler { get; set; } = null;


        private IDictionary<string, Func<TTarget,object>> DerivedProperties { get; } = new ConcurrentDictionary<string, Func<TTarget, object>>();
        public void AddDerived([NotNull] string propertyName, [NotNull] Func<TTarget,object> builder )
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(propertyName));

            DerivedProperties[propertyName] = builder ?? throw new ArgumentNullException(nameof(builder));
        }


        private ReaderWriterLockSlim Lock { get; } = new ReaderWriterLockSlim();


        public bool HasValue([NotNull] string propertyName )
        {

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(propertyName));

            if( CurrentState.ContainsKey( propertyName ) )
                return true;

            if( Target.GetType().GetProperty( propertyName ) != null )
                return true;

            if( DerivedProperties.ContainsKey( propertyName ) )
                return true;

            return false;

        }



        public object Get([NotNull] string propertyName )
        {

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(propertyName));

            try
            {

                Lock.EnterReadLock();



                // First check the current state for the property
                // This handles both the mutable and immutable cases
                if (CurrentState.TryGetValue(propertyName, out var value))
                    return value;

                // Reflect the target to find the property
                var info = Target.GetType().GetProperty(propertyName);
                if (info != null)
                    return info.GetValue(Target, new object[] { });

                // Now check for a derived property
                if (DerivedProperties.TryGetValue(propertyName, out var builder))
                    return builder(Target);


                // Use the missing property handler if one is defined
                // The mere definition of a handler persumes that
                // a value will be produced even if it is null
                // thus true is always returned
                if (MissingPropertyHandler != null)
                    return MissingPropertyHandler(Target, propertyName);

                throw new InvalidOperationException($"Property {propertyName} could not be resolved");


            }
            finally
            {
                Lock.ExitReadLock();                
            }
        }


        public TType Get<TType>([NotNull] string propertyName )
        {

            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            var value = Get( propertyName );
            return value.As<TType>();
        }


        public object Get( [NotNull] string propertyName, [CanBeNull] object defaultValue )
        {

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(propertyName));

            try
            {
                return Get( propertyName );
            }
            catch
            {
                return defaultValue;
            }
        }


        private IDictionary<string, object> CurrentState { get; } = new ConcurrentDictionary<string, object>();

        public override bool TryGetMember( GetMemberBinder binder, out object result )
        {

            try
            {

                result = Get(binder.Name);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }

        }


        public void Set([NotNull] string propertyName, [CanBeNull] object value )
        {

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(propertyName));

            try
            {

                Lock.EnterWriteLock();

                // Tracking is not being used apply the update directly to the underlying target
                if (!(Tracked))
                {
                    var ok = _Update(propertyName, value);
                    if (ok)
                        return;
                }

                if (DerivedProperties.ContainsKey(propertyName))
                    throw new InvalidOperationException($"An expando property can not override a defined derived property: Name={propertyName}");

                CurrentState[propertyName] = value;            

            }
            finally
            {
                Lock.ExitWriteLock();                
            }
        }

        public override bool TrySetMember([NotNull] SetMemberBinder binder, object value)
        {
            Set( binder.Name, value );
            return true;
        }



        public override bool TryInvokeMember([NotNull] InvokeMemberBinder binder, object[] args, out object result)
        {

            if (binder.Name == "As" && (args.Length == 2))
            {

                var name = args[0] as string ?? "";
                var type = args[1] as Type??typeof(object);
                
                var info = Target.GetType().GetProperty(name);
                var x    = info.GetValue(Target, new object[] { });

                result = x.As(type);
                return true;

            }

            if( binder.Name == "Undo" && (args.Length == 0) )
            {
                Undo();
                result = null;
                return true;
            }

            if( binder.Name == "Post" && (args.Length == 0) )
            {
                Post();
                result = null;
                return true;
            }

            return base.TryInvokeMember(binder, args, out result);

        }


        private bool _Update([NotNull] string propertyName, object value )
        {

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(propertyName));

            var info = Target.GetType().GetProperty( propertyName );
            if( info != null )
            {
                var x = value.As( info.PropertyType );
                info.SetValue( Target, x, new object[] {} );
                return true;
            }

            return false;

        }    


        public void Undo()
        {
            try
            {
                Lock.EnterWriteLock();

                CurrentState.Clear();

            }
            finally
            {
                Lock.ExitWriteLock();
            }

        }


        public void Post()
        {

            try
            {

                Lock.EnterWriteLock();

                foreach (var p in CurrentState)
                    _Update(p.Key, p.Value);

                CurrentState.Clear();

            }
            finally
            {
                Lock.ExitWriteLock();
            }

        }


    }

}