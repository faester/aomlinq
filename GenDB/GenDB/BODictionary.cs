using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;
using System.Collections;
using System.Query;
using System.Expressions;

namespace GenDB
{
    /// <summary>
    /// Used internally in the BODictionary to keep track of 
    /// how the keys/values are mapped to the BODictionary 
    /// in the database.
    /// </summary>
    internal class DictKeyValueMapping : AbstractBusinessObject
    {
        int keyListId;

        public int KeyListId
        {
            get { return keyListId; }
            set { keyListId = value; }
        }

        int valueListId;

        public int ValueListId
        {
            get { return valueListId; }
            set { valueListId = value; }
        }

        int dictionaryId;

        public int DictionaryId
        {
            get { return dictionaryId; }
            set { dictionaryId = value; }
        }
    }

    /// <summary>
    /// Gem typerne for K og V som properties på objektet og giv disse faste 
    /// navne index TypeSystem
    /// TODO: BODictionary, gem i 2 bolist - skal huske entityPOID på dem så de kan retrieves.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public sealed class BODictionary<K, V> : AbstractBusinessObject, IDictionary<K, V>, IDBSaveableCollection
    {
        static Table<DictKeyValueMapping> mappings = DataContext.Instance.CreateTable<DictKeyValueMapping>();
        static Table<BOList<K>> keyLists = DataContext.Instance.CreateTable<BOList<K>>();
        static Table<BOList<V>> valueLists = DataContext.Instance.CreateTable<BOList<V>>();

        Dictionary<K, V> dict = new Dictionary<K,V>();
        DictKeyValueMapping mapping = null;

        bool isDictionaryPopulated = false;
        bool isReadOnly = false;
        bool hasBeenModified = false;

        /// <summary>
        /// </summary>
        public BODictionary()
        {
        }

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { isReadOnly = value; }
        }

        public bool HasBeenModified
        {
            get { return hasBeenModified; }
            set { hasBeenModified = value; }
        }

        private void PopulateDictionary()
        {
            if (!DBIdentity.IsPersistent) { return; }

            var k = (from m in mappings
                    where m.DictionaryId == DBIdentity.Value
                    select m).First();

            BOList<V> values = (BOList<V>)DataContext.Instance.GenDB.GetByEntityPOID (k.ValueListId);
            BOList<K> keys = (BOList<K>)DataContext.Instance.GenDB.GetByEntityPOID (k.KeyListId);

            for (int i = 0; i < values.Count; i++)
            {
                dict[keys[i]] = values[i];
            }
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
            HasBeenModified=true;
        }

        public bool Remove(K key)
        {
            TestPopulateDictionary();
            HasBeenModified=true;
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
                HasBeenModified=true;
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
            HasBeenModified=true;
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
            HasBeenModified=true;
        }

        public bool Remove(KeyValuePair<K, V> kvp)
        {
            TestPopulateDictionary();
            if (Contains(kvp))
            {
                dict.Remove (kvp.Key);
                HasBeenModified=true;
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
            set{return;}
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