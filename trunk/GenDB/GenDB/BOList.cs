using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;
using System.Collections;

namespace GenDB
{

    public class BOListFactory
    {
        TypeSystem typeSystem;
        MappingType intMapping;
        MappingType longMapping;
        MappingType dtMapping;
        MappingType charMapping;
        MappingType boolMapping;
        MappingType strMapping;
        MappingType doubleMapping;
        MappingType floatMapping;

        internal BOListFactory(TypeSystem tsys)
        {
            typeSystem = tsys;
            Init();
        }

        void Init()
        {
            intMapping = typeSystem.FindMappingType(typeof(int));
            longMapping = typeSystem.FindMappingType(typeof(long));
            dtMapping = typeSystem.FindMappingType(typeof(DateTime));
            charMapping = typeSystem.FindMappingType(typeof(char));
            boolMapping = typeSystem.FindMappingType(typeof(bool));
            strMapping = typeSystem.FindMappingType(typeof(string));
            doubleMapping = typeSystem.FindMappingType(typeof(double));
            floatMapping = typeSystem.FindMappingType(typeof(float));
        }

        public BOList<T> BOListRef<T>()
            where T : IBusinessObject
        {
            return new BOList<T>(MappingType.REFERENCE, typeSystem);
        }

        public BOList<int> BOListInt() { return new BOList<int>(intMapping, typeSystem); }
        public BOList<string> BOListString() { return new BOList<string>(strMapping, typeSystem); }
        public BOList<DateTime> BOListDateTime() { return new BOList<DateTime>(dtMapping, typeSystem); }
        public BOList<long> BOListLong() { return new BOList<long>(longMapping, typeSystem); }
        public BOList<bool> BOListBool() { return new BOList<bool>(boolMapping, typeSystem); }
        public BOList<char> BOListChar() { return new BOList<char>(charMapping, typeSystem); }
        public BOList<double> BOListDouble() { return new BOList<double>(doubleMapping, typeSystem); }
        public BOList<float> BOListFloat() { return new BOList<float>(floatMapping, typeSystem); }
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
        TypeSystem typeSystem;

        /// <summary>
        /// Hide constructor to prevent instantiation 
        /// of unrestricted type parameter.
        /// </summary>
        internal BOList(MappingType mappingType, TypeSystem typeSystem)
        {
            this.typeSystem = typeSystem;
            mt = mappingType;
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
                CollectionElementConverter cnv = new CollectionElementConverter(mt, DataContext.Instance);
                
                foreach (IGenCollectionElement ce in DataContext.Instance.GenDB.AllElements(DBTag.EntityPOID))
                {
                    T value = (T)cnv.Translate (ce);
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

            CollectionElementConverter cnv = new CollectionElementConverter(mt, DataContext.Instance);

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
