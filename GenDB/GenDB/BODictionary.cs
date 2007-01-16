using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Collections;

namespace GenDB
{
    /// <summary>
    /// Gem typerne for K og V som properties på objektet og giv disse faste 
    /// navne index TypeSystem
    /// TODO: BODictionary
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class BODictionary<K, V> : AbstractBusinessObject, IDictionary<K, V>, IDBSaveableCollection
    {
        Dictionary<K, V> dict = null;
        bool isDictionaryPopulated = false;
        bool isReadOnly = false;

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { isReadOnly = value; }
        }

        private void PopulateDictionary()
        {
            throw new Exception("Not implemented");
        }

        private void TestPopulateDictionary()
        {
            if (!isDictionaryPopulated)
            {
                PopulateDictionary();
                isDictionaryPopulated  = true;
            }
        }

        public void SaveElementsToDB()
        {
            if (!isDictionaryPopulated)
            {
                return;
            }
            if (!DBIdentity.IsPersistent)
            {
                throw new Exception("Attempted to save elements prior to saving the BODictionary");
            }
            throw new Exception("Not implemented");
        }

        public bool ContainsKey(K key)
        {
            TestPopulateDictionary();
            return dict.ContainsKey(key);
        }

        public void Add(K key, V value)
        {
            TestPopulateDictionary();
            dict.Add (key, value);
        }

        public bool Remove(K key)
        {
            TestPopulateDictionary();
            return dict.Remove (key);
        }

        public bool TryGetValue(K key, out V value)
        {
            TestPopulateDictionary();
            return dict.TryGetValue (key, out value);
        }

        public V this[K key]
        {
            get { 
                TestPopulateDictionary();
                return dict[key];
            }
            set { 
                TestPopulateDictionary();
                dict[key] = value;
            }
        }

        public ICollection<K> Keys
        {
            get { 
                TestPopulateDictionary();
                return dict.Keys;
            }
        }

        public ICollection<V> Values
        {
            get { 
                TestPopulateDictionary();
                return dict.Values;
            }
        }

        public void Add(KeyValuePair<K, V> kvp)
        {
            TestPopulateDictionary();
            dict.Add(kvp.Key, kvp.Value);
        }

        public void Clear()
        {
            TestPopulateDictionary();
            dict.Clear();
        }

        public bool Contains(KeyValuePair<K, V> kvp)
        {
            TestPopulateDictionary();
            V testValue;
            if (!dict.TryGetValue (kvp.Key, out testValue))
            {
                return false;
            }
            else
            {
                return Comparer.Equals(testValue, kvp.Value);
            }
        }

        public void CopyTo(KeyValuePair<K, V>[] kvps, int idx)
        {
            TestPopulateDictionary();
            foreach (KeyValuePair<K, V> kvp in dict)
            {
                kvps[idx++] = kvp;
            }
        }

        public bool Remove(KeyValuePair<K, V> kvp)
        {
            TestPopulateDictionary();
            if (Contains(kvp))
            {
                dict.Remove (kvp.Key);
                return true;
            }
            else
            {
                return false;
            }
        }

        public int Count
        {
            get { 
                TestPopulateDictionary();
                return dict.Count;
            }
        }

        public System.Collections.Generic.IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            TestPopulateDictionary();
            return dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}