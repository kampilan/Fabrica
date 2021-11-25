using System.Collections;
using System.Collections.Generic;

namespace Fabrica.Utilities.Types
{

    
    public class NoNullDictionary: IDictionary<string,object>
    {

        public NoNullDictionary()
        {

        }

        public NoNullDictionary(IDictionary<string, object> src )
        {
            Inner = new Dictionary<string, object>(src);
        }


        private IDictionary<string, object> Inner { get; } = new Dictionary<string, object>();


        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Inner).GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            if( item.Value is not null )
                Inner.Add(item);
        }

        public void Clear()
        {
            Inner.Clear();
        }

        public bool Contains( KeyValuePair<string, object> item )
        {
            return Inner.Contains(item);
        }

        public void CopyTo( KeyValuePair<string, object>[] array, int arrayIndex )
        {
            Inner.CopyTo(array, arrayIndex);
        }

        public bool Remove( KeyValuePair<string, object> item )
        {
            return Inner.Remove(item);
        }

        public int Count => Inner.Count;

        public bool IsReadOnly => Inner.IsReadOnly;

        public void Add( string key, object value )
        {
            if( value is not null )
                Inner.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return Inner.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return Inner.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return Inner.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get => Inner[key];
            set => Add(key,value);
        }


        public ICollection<string> Keys => Inner.Keys;

        public ICollection<object> Values => Inner.Values;


    }


}
