using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fabrica.Models.Support
{

    public class DeltaPropertySet : IDictionary<string, object>
    {

        public DeltaPropertySet()
        {

        }

        public DeltaPropertySet( BaseDelta delta )
        {

            if (delta is null)
                return;

            foreach( var pi in delta.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead) )
            {
                var value = pi.GetValue(delta, null);
                if( value is not null )
                    Inner[pi.Name] = value;
            }


        }


        private IDictionary<string, object> Inner { get; } = new Dictionary<string, object>();


        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Inner).GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            if (item.Value is not null)
                Inner.Add(item);
        }

        public void Clear()
        {
            Inner.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Inner.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            Inner.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Inner.Remove(item);
        }

        public int Count => Inner.Count;

        public bool IsReadOnly => Inner.IsReadOnly;

        public void Add(string key, object value)
        {
            if (value is not null)
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
            set => Add(key, value);
        }


        public ICollection<string> Keys => Inner.Keys;

        public ICollection<object> Values => Inner.Values;




    }


}
