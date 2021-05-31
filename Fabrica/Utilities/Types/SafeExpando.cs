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

using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using JetBrains.Annotations;

namespace Fabrica.Utilities.Types
{


    public class SafeExpando : DynamicObject, IDictionary<string,object>
    {

        public SafeExpando()
        {
            
        }

        public SafeExpando( [NotNull] IEnumerable<KeyValuePair<string, object>> source )
        {

            foreach( var pair in source )
                _values.Add( pair );

        }


        private readonly IDictionary<string, object> _values = new Dictionary<string, object>();


        public override bool TryGetMember([NotNull] GetMemberBinder binder, out object result)
        {

            result = !(_values.TryGetValue( binder.Name, out var member )) ? new SafeProperty( null ) : new SafeProperty( member );

            return true;

        }

        public override bool TrySetMember( SetMemberBinder binder, [CanBeNull] object value )
        {

            if( value != null )
                _values[binder.Name] = value;
                
            return true;

        }


        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_values).GetEnumerator();
        }

        void ICollection<KeyValuePair<string, object>>.Add( KeyValuePair<string, object> item )
        {
            _values.Add( item );
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            _values.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains( KeyValuePair<string, object> item )
        {
            return _values.Contains( item );
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo( KeyValuePair<string, object>[] array, int arrayIndex )
        {
            _values.CopyTo( array, arrayIndex );
        }

        bool ICollection<KeyValuePair<string, object>>.Remove( KeyValuePair<string, object> item )
        {
            return _values.Remove( item );
        }

        int ICollection<KeyValuePair<string, object>>.Count => _values.Count;

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => _values.IsReadOnly;

        bool IDictionary<string, object>.ContainsKey( string key )
        {
            return _values.ContainsKey( key );
        }

        void IDictionary<string, object>.Add( string key, object value )
        {
            _values.Add( key, value );
        }

        bool IDictionary<string, object>.Remove( string key )
        {
            return _values.Remove( key );
        }

        bool IDictionary<string, object>.TryGetValue( string key, out object value )
        {
            return _values.TryGetValue( key, out value );
        }

        object IDictionary<string, object>.this[ string key ]
        {
            get => _values[key];
            set => _values[key] = value;
        }

        ICollection<string> IDictionary<string, object>.Keys => _values.Keys;

        ICollection<object> IDictionary<string, object>.Values => _values.Values;
    }


}
