using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;
using System.Collections;

namespace GenDB
{

    public class BOListFactory
    {
        internal BOListFactory()
        {
        }

        public BOList<T> BOListRef<T>()
            where T : IBusinessObject
        {
            return new BOList<T>();
        }

        public BOList<int> BOListInt() { return new BOList<int>(); }
        public BOList<string> BOListString() { return new BOList<string>(); }
        public BOList<DateTime> BOListDateTime() { return new BOList<DateTime>(); }
        public BOList<long> BOListLong() { return new BOList<long>(); }
        public BOList<bool> BOListBool() { return new BOList<bool>(); }
        public BOList<char> BOListChar() { return new BOList<char>(); }
        public BOList<double> BOListDouble() { return new BOList<double>(); }
        public BOList<float> BOListFloat() { return new BOList<float>(); }
    }

    /// <summary>
    /// BOList has identical functionality to a regular list, but 
    /// can be stored in the generic db system. This type can not 
    /// be instantiated, since there are no restrictions on the 
    /// parameters, and the GenDB system only supports storage of 
    /// reference types, if they implement IBusinessObject.
    /// 
    /// To create instances, use the BOListFactory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class BOList<T> : AbstractBusinessObject, IList<T>, IDBSaveableCollection
    {
        List<T> theList = new List<T>();
        bool isReadOnly;
        bool isListPopulated = false;
        MappingType mt;
        CollectionElementConverter cnv = null;
                

        /// <summary>
        /// Hide constructor to prevent instantiation 
        /// of unrestricted type parameter.
        /// </summary>
        internal BOList()
        {
            mt = DataContext.Instance.TypeSystem.FindMappingType(typeof(T));
            cnv = new CollectionElementConverter(mt, DataContext.Instance, typeof(T));
        }

        private void Set(int idx, T t)
        {
            if (theList.Count <= idx)
            {
                for (int i = theList.Count; i <= idx; i++)
                {
                    theList.Add(default(T));
                }
            }
            
            theList[idx] = t;
        }

        private void RetrieveElements()
        {
            isListPopulated = true;

            /* 
             * Elements can only exist in the DB
             * if this BOList element has been 
             * persisted. Hence if DBTag is null, 
             * there is nothing to load.
             */

            if (DBTag != null)
            {
                foreach (IGenCollectionElement ce in DataContext.Instance.GenDB.AllElements(DBTag.EntityPOID))
                {
                    object o = cnv.PickCorrectElement(ce);
                    T value = (T)o;
                    Set(ce.ElementIndex, value);
                }
            }
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
                if (!isListPopulated) { this.RetrieveElements(); }
                return theList[idx];
            }
            set
            {
                if (!isListPopulated) { this.RetrieveElements(); }
                theList[idx] = value;
            }
        }

        public int IndexOf(T t)
        {
            if (!isListPopulated) { this.RetrieveElements(); }
            return theList.IndexOf(t);
        }

        public void Insert(int idx, T item)
        {
            if (!isListPopulated) { this.RetrieveElements(); }
            theList.Insert(idx, item);
        }

        public void RemoveAt(int idx)
        {
            if (!isListPopulated) { this.RetrieveElements(); }
            theList.RemoveAt(idx);
        }

        public void Add(T item)
        {
            if (!isListPopulated) { this.RetrieveElements(); }
            theList.Add(item);
        }

        public void Clear()
        {
            if (!isListPopulated) { this.RetrieveElements(); }
            theList.Clear();
        }

        public bool Contains(T item)
        {
            if (!isListPopulated) { this.RetrieveElements(); }
            return theList.Contains(item);
        }

        public void CopyTo(T[] arr, int idx)
        {
            if (!isListPopulated) { this.RetrieveElements(); }
            theList.CopyTo(arr, idx);
        }

        public bool Remove(T item)
        {
            if (!isListPopulated) { this.RetrieveElements(); }
            return theList.Remove(item);
        }

        public int Count
        {
            get
            {
                if (!isListPopulated) { this.RetrieveElements(); }
                return theList.Count;
            }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            if (!isListPopulated) { this.RetrieveElements(); }
            return theList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Saves the elements to the database. This is handled 
        /// internally, why the public access modifier imposes
        /// an encapsulation problem. 
        /// </summary>
        public void SaveElementsToDB()
        {
            if (!isListPopulated)
            {
                return; //Since nothing has been loaded, nothing needs to be saved.
            } 

            GenDB.DB.IGenericDatabase db = DataContext.Instance.GenDB;
            long thisPOID = this.DBTag.EntityPOID;
            db.ClearCollection(thisPOID);

            CollectionElementConverter cnv = new CollectionElementConverter(mt, DataContext.Instance, typeof(T));

            Type elementType = typeof(T);

            for (int idx = 0; idx < theList.Count; idx++)
            {
                IGenCollectionElement ce = cnv.Translate(theList[idx]);
                ce.ElementIndex = idx;
                db.Save(ce, thisPOID, mt);
            }
        }
    }

}
