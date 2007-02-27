using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;
using System.Collections;
using System.Query;
using System.Expressions;

namespace GenDB
{
    public abstract class IDictionaryStub : AbstractBusinessObject, IDBSaveableCollection
    {
        DictKeyValueMapping mapping = null;
        
        [Volatile]
        public DictKeyValueMapping Mapping
        {
            get { return mapping; }
            set { mapping = value; }
        }

        public abstract void SaveElementsToDB();

        bool hasBeenModified = false;
        
        [Volatile]
        public bool HasBeenModified
        {
            get { return hasBeenModified; }
            set { hasBeenModified = value; }
        }
    }

    /// <summary>
    /// Used internally in the BODictionary to keep track of 
    /// how the keys/values are mapped to the BODictionary 
    /// in the database.
    /// </summary>
    public class DictKeyValueMapping : AbstractBusinessObject
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
    public sealed class BODictionary<K, V> : IDictionaryStub, IDictionary<K, V>    
    {
        static Table<DictKeyValueMapping> dkv_mappings = DataContext.Instance.GetTable<DictKeyValueMapping>();
        //static Table<InternalList<K>> keyLists = DataContext.Instance.GetTable<InternalList<K>>();
        //static Table<InternalList<V>> valueLists = DataContext.Instance.GetTable<InternalList<V>>();
        IIBoToEntityTranslator superTranslator = null;

        Dictionary<K, V> dict = new Dictionary<K,V>();
        InternalList<K> keys = null;
        InternalList<V> values = null;

        bool isDictionaryPopulated = false;
        bool isReadOnly = false;

        /// <summary>
        /// </summary>
        public BODictionary() { }

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { isReadOnly = value; }
        }

        private void PopulateDictionary()
        {
            if (!DBIdentity.IsPersistent) { return; }

            DictKeyValueMapping k = Mapping;

            var q = from m in dkv_mappings
                    where m.DictionaryId == DBIdentity.Value
                    select m;

            foreach(DictKeyValueMapping map in q)
            {
                k = map;
            }

            if (k == null)
            {
                return;
            }

            InternalList<V> values = (InternalList<V>)DataContext.Instance.GenDB.GetByEntityPOID (k.ValueListId);
            InternalList<K> keys = (InternalList<K>)DataContext.Instance.GenDB.GetByEntityPOID (k.KeyListId);

            if (values == null) { throw new NullReferenceException("values"); }
            if (keys == null) { throw new NullReferenceException("keys"); }

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

        public override void SaveElementsToDB()
        {
            if (!isDictionaryPopulated)
            {
                return;
            }

            InternalList<K> keys;
            InternalList<V> values;
            if (Mapping == null)
            {
                Mapping = new DictKeyValueMapping();
                keys = new InternalList<K>();
                values = new InternalList<V>();
            }
            else
            {
                keys = (InternalList<K>)DataContext.Instance.GenDB.GetByEntityPOID(Mapping.KeyListId);
                values = (InternalList<V>)DataContext.Instance.GenDB.GetByEntityPOID(Mapping.ValueListId);
                if (keys == null || values == null) 
                {
                    keys = new InternalList<K>();
                    values = new InternalList<V>();
                }
            }

            values.Clear();
            keys.Clear();

            int idx = 0;
            foreach(KeyValuePair <K, V> kvp in dict)
            {
                values.Add(kvp.Value);
                keys.Add(kvp.Key);
                idx++;
            }

            TypeSystem ts = DataContext.Instance.TypeSystem;

            if (!ts.IsTypeKnown(typeof(InternalList<V>)))
            {
                ts.RegisterType(typeof(InternalList<V>));
            }
            if (!ts.IsTypeKnown(typeof(InternalList<K>)))
            {
                ts.RegisterType(typeof(InternalList<K>));
            }
            

            IIBoToEntityTranslator vt = DataContext.Instance.Translators.GetTranslator(values.GetType());
            IIBoToEntityTranslator kt = DataContext.Instance.Translators.GetTranslator(keys.GetType());

            vt.SaveToDB(values);
            kt.SaveToDB(keys);

            Mapping.KeyListId = keys.DBIdentity;
            Mapping.ValueListId = values.DBIdentity;
            Mapping.DictionaryId = DBIdentity;
            
            IEntity ie = DataContext.Instance.GenDB.NewEntity();
            DataContext.Instance.IBOCache.Add(Mapping, ie.EntityPOID);
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
                HasBeenModified=true;
                dict[key] = value;
            }
        }

        [Volatile]
        public ICollection<K> Keys
        {
            get { 
                TestPopulateDictionary();
                return dict.Keys;
            }
        }

        [Volatile]
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

        [Volatile]
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

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            BODictionary<K, V> that = (obj as BODictionary <K, V>);
            if (that == null) { return false; }

            if (that.Count != this.Count) { return false; }

            foreach(KeyValuePair <K, V> kvp in this)
            {
                V otherValue;
                if (!that.TryGetValue(kvp.Key, out otherValue)) { return false; }
                if (!kvp.Value.Equals (otherValue)) { return false;}
            }
            return true;
        }
    }
}