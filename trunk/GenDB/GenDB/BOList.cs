using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;
using System.Collections;

namespace GenDB
{
    /// <summary>
    /// BOList has identical functionality to a regular list, but 
    /// can be stored in the generic db system. Elements are retrieved
    /// lazely, that is, upon first attempt to traverse the collection
    /// or access an indexed element. When one of these operations are
    /// performed, all elements are retrieved.
    /// 
    /// There are some problem with the generic type specifications: We 
    /// want to allow only lists of either primitive elements (+ string 
    /// and Datatime) or IBusinessObjects, but this can not be expressed 
    /// with C# generic type constraints.
    /// 
    /// Thus it is in fact possible to instantiate BOList object, that will 
    /// cause exceptions, if one attempts to store them in the db. We have 
    /// considered using various workarounds (constrained factory methods, 
    /// making the list internal and providing public subclasses specialized
    /// for each primitive and one for all IBusinessObject's etc). We did 
    /// how ever decide to refrain from the compiler type checking, since 
    /// the introduction of a number of new types would make it much more
    /// inconvenient to use the class on the application side.
    /// 
    /// The above considerations are even more appropriate for the BODictionary.
    /// 
    /// To create instances, use the BOListFactory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class BOList<T> : AbstractBusinessObject, IList<T>, IDBSaveableCollection
    {
        List<T> theList = new List<T>();
        bool isReadOnly;
        bool isListPopulated = false;
        bool hasBeenModified = false;
        MappingType mt;
        CollectionElementConverter cnv = null;
         
        /// <summary>
        /// Hide constructor to prevent instantiation 
        /// of unrestricted type parameter.
        /// </summary>
        public BOList()
        {
            mt = TypeSystem.FindMappingType(typeof(T));
            cnv = new CollectionElementConverter(mt, DataContext.Instance, typeof(T));
        }

        ///<summary>
        ///Set indexed element. Will enhance list to ensure capacity 
        ///to store element. The method is private, and is only used 
        ///when retrieving values from db.
        ///</summary>
        private void Set(int idx, T t)
        {
            if (theList.Count <= idx) {
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
             * persisted. Hence if IsPersistent 
             * is false, there is nothing to load.
             */
            if (DBIdentity.IsPersistent)
            {
                foreach (IGenCollectionElement ce in DataContext.Instance.GenDB.AllElements(DBIdentity))
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

        public bool HasBeenModified
        {
            get { return hasBeenModified; }
            set { hasBeenModified = value; }
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
            HasBeenModified=true;
        }

        public void RemoveAt(int idx)
        {
            if (!isListPopulated) { this.RetrieveElements(); }
            theList.RemoveAt(idx);
            HasBeenModified=true;
        }

        public void Add(T item)
        {
            if (!isListPopulated) { this.RetrieveElements(); }
            theList.Add(item);
            HasBeenModified=true;
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
            HasBeenModified=true;
        }

        public bool Remove(T item)
        {
            if (!isListPopulated) { this.RetrieveElements(); }
            HasBeenModified=true;
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
            int thisPOID = this.DBIdentity;
            db.ClearCollection(thisPOID);

            CollectionElementConverter cnv = new CollectionElementConverter(mt, DataContext.Instance, typeof(T));

            Type elementType = typeof(T);

            for (int idx = 0; idx < theList.Count; idx++)
            {
                IGenCollectionElement ce = cnv.Translate(theList[idx]);
                ce.ElementIndex = idx;
                db.Save(ce, thisPOID, mt);
            }
            HasBeenModified=false;
        }
    }
}
