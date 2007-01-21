using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Collections;

namespace GenDB
{

    public class BODictionaryFactory
    {
        internal BODictionaryFactory()
        {
        }

        public BODictionary<K, V> BODictionaryRef<K, V>()
            where V : IBusinessObject
        {
            return new BODictionary<K, V>();
        }

        public BODictionary<K, int> BODictionaryInt<K>() {return new BODictionary<K, int>();}
        public BODictionary<K, string> BODictionaryString<K>() {return new BODictionary<K, string>();}
        public BODictionary<K, DateTime> BODictionaryDateTime<K>() {return new BODictionary<K, DateTime>();}
        public BODictionary<K, long> BODictionaryLong<K>() {return new BODictionary<K, long>();}
        public BODictionary<K, bool> BODictionaryBool<K>() {return new BODictionary<K, bool>();}
        public BODictionary<K, char> BODictionaryChar<K>() {return new BODictionary<K, char>();}
        public BODictionary<K, double> BODictionaryDouble<K>() {return new BODictionary<K, double>();}
        public BODictionary<K, float> BODictionaryFloat<K>() {return new BODictionary<K, float>();}
    }

    internal class KeyDict<K>
    {
        BOList<K> keyList = new BOList<K>();
        
        public BOList<K> KeyList
        {
            get{return keyList;}
            set{keyList=value;}
        }

        public DBIdentifier GetDBID()
        {
            return keyList.DBIdentity;
        }
    }

    internal class ValueDict<V>
    {
        BOList<V> valueList = new BOList<V>();

        public BOList<V> ValueList
        {
            get{return valueList;}
            set{valueList=value;}
        }

        public DBIdentifier GetDBID()
        {
            return valueList.DBIdentity;
        }
    }

    /// <summary>
    /// Gem typerne for K og V som properties på objektet og giv disse faste 
    /// navne index TypeSystem
    /// TODO: BODictionary, gem i 2 bolist - skal huske entityPOID på dem så de kan retrieves.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class BODictionary<K, V> : AbstractBusinessObject, IDictionary<K, V>, IDBSaveableCollection
    {
        Dictionary<K, V> dict = new Dictionary<K,V>();

        KeyDict<K> keyDict = new KeyDict<K>();
        ValueDict<V> valueDict = new ValueDict<V>();

        //BOList<K> keyList = new BOList<K>();
        //BOList<V> valueList = new BOList<V>();

        bool isDictionaryPopulated = false;
        bool isReadOnly = false;

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { isReadOnly = value; }
        }
        
        private void PopulateDictionary()
        {
            //throw new Exception("Not implemented");
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
            //if (!DBIdentity.IsPersistent)
            //{
            //    throw new Exception("Attempted to save elements prior to saving the BODictionary");
            //}
            
            foreach(KeyValuePair<K, V> kvp in dict)
            {
                keyDict.KeyList.Add(kvp.Key);
                valueDict.ValueList.Add(kvp.Value);
            }
            keyDict.KeyList.SaveElementsToDB();
            valueDict.ValueList.SaveElementsToDB();
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