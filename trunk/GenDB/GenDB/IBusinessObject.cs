using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Query;
using System.Expressions;

namespace GenDB
{
    public interface IBusinessObject
    {
        /// <summary>
        /// NB: The DBTag is essential to the persistence 
        /// system and should not be referenced or 
        /// modified by the user!
        /// </summary>
        DBTag DBTag { get; set; }
    }


    /// <summary>
    /// BOList has identical functionality to a regular list, but can be stored in the generic db system.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class BOList<T> : AbstractBusinessObject, IList<T>
    {
        List<T> theList = new List<T>();
        bool isReadOnly;
        bool isElementsRetrieved = false;

        private void RetrieveElements()
        {
            isElementsRetrieved = true;
            throw new Exception("Not implemented.");
        }

        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set { isReadOnly = value; }
        }

        public T this[int idx]
        {
            get
            {
                if (!isElementsRetrieved) { this.RetrieveElements(); }
                return theList[idx];
            }
            set
            {
                if (!isElementsRetrieved) { this.RetrieveElements(); }
                theList[idx] = value;
            }
        }

        public int IndexOf(T t)
        {
            if (!isElementsRetrieved) { this.RetrieveElements(); }
            return theList.IndexOf(t);
        }

        public void Insert(int idx, T item)
        {
            if (!isElementsRetrieved) { this.RetrieveElements(); }
            theList.Insert(idx, item);
        }

        public void RemoveAt(int idx)
        {
            if (!isElementsRetrieved) { this.RetrieveElements(); }
            theList.RemoveAt(idx);
        }

        public void Add(T item)
        {
            if (!isElementsRetrieved) { this.RetrieveElements(); }
            theList.Add(item);
        }

        public void Clear()
        {
            if (!isElementsRetrieved) { this.RetrieveElements(); }
            theList.Clear();
        }

        public bool Contains(T item)
        {
            if (!isElementsRetrieved) { this.RetrieveElements(); }
            return theList.Contains(item);
        }

        public void CopyTo(T[] arr, int idx)
        {
            if (!isElementsRetrieved) { this.RetrieveElements(); }
            theList.CopyTo(arr, idx);
        }

        public bool Remove(T item)
        {
            if (!isElementsRetrieved) { this.RetrieveElements(); }
            return theList.Remove(item);
        }

        public int Count
        {
            get
            {
                if (!isElementsRetrieved) { this.RetrieveElements(); }
                return theList.Count;
            }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            if (!isElementsRetrieved) { this.RetrieveElements(); }
            return theList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Gem typerne for K og V som properties på objektet og giv disse faste 
    /// navne i TypeSystem
    /// TODO: BODictionary
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class BODictionary<K, V> : Dictionary<K, V>, IBusinessObject
    {
        DBTag dbtag;

        public DBTag DBTag
        {
            get { return dbtag; }
            set { dbtag = value; }
        }
    }
}
