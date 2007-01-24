using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;
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

        // key kan kun være int da der var for mange kombinationer til at det virkede fornuftigt
        // at fortsætte denne implementation.
        public BODictionary<int, int> BODictionaryInt() {return new BODictionary<int, int>();}
        public BODictionary<int, string> BODictionaryString() {return new BODictionary<int, string>();}
        public BODictionary<int, DateTime> BODictionaryDateTime() {return new BODictionary<int, DateTime>();}
        public BODictionary<int, long> BODictionaryLong() {return new BODictionary<int, long>();}
        public BODictionary<int, bool> BODictionaryBool() {return new BODictionary<int, bool>();}
        public BODictionary<int, char> BODictionaryChar() {return new BODictionary<int, char>();}
        public BODictionary<int, double> BODictionaryDouble() {return new BODictionary<int, double>();}
        public BODictionary<int, float> BODictionaryFloat() {return new BODictionary<int, float>();}
    }

    internal class KeyDict<K>
    {
        BOList<K> keyList = new BOList<K>();
        int dBIdentifier;

        public KeyDict() 
        {
            dBIdentifier=keyList.DBIdentity.Value;
        }

        public BOList<K> KeyList
        {
            get{return keyList;}
            set{keyList=value;}
        }

        public int DBIdentifier
        {
            get{return dBIdentifier;}
        }
    }

    internal class ValueDict<V>
    {
        BOList<V> valueList = new BOList<V>();
        int dBIdentifier;

        public ValueDict()
        {
            dBIdentifier=valueList.DBIdentity.Value;
        }

        public BOList<V> ValueList
        {
            get{return valueList;}
            set{valueList=value;}
        }

        public int DBIdentifier
        {
            get{return dBIdentifier;}
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
        Dictionary<K, V> dict = new Dictionary<K,V>();

        KeyDict<K> keyDict = new KeyDict<K>();
        ValueDict<V> valueDict = new ValueDict<V>();

        bool isDictionaryPopulated = false;
        bool isReadOnly = false;
        bool hasBeenModified = false;
        MappingType mt_key;
        MappingType mt_value;
        CollectionElementConverter cnv_key = null;
        CollectionElementConverter cnv_value = null;

        /// <summary>
        /// Hide constructor to prevent instantiation 
        /// of unrestricted type parameter.
        /// </summary>
        internal BODictionary()
        {
            mt_key = DataContext.Instance.TypeSystem.FindMappingType(typeof(K));
            mt_value = DataContext.Instance.TypeSystem.FindMappingType(typeof(V));
            cnv_key = new CollectionElementConverter(mt_key, DataContext.Instance, typeof(K));
            cnv_value = new CollectionElementConverter(mt_value, DataContext.Instance, typeof(V));
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
            HasBeenModified=false;
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
            return dict.Remove (key);
            HasBeenModified=true;
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
            set{return;}
        }

        public ICollection<V> Values
        {
            get { 
                TestPopulateDictionary();
                return dict.Values;
            }
            set{return;}
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