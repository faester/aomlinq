using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Query;
using System.Expressions;
using GenDB.DB;

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

    internal interface IDBSaveableCollection
    {
        void SaveElementsToDB();
    }

    /// <summary>
    /// BOList has identical functionality to a regular list, but can be stored in the generic db system.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class BOList<T> : AbstractBusinessObject, IList<T>, IDBSaveableCollection
    {
        List<T> theList = new List<T>();
        bool isReadOnly;
        bool isElementsRetrieved = false;
        MappingType mt;

        public BOList()
        {
            mt = TypeSystem.FindMappingType (typeof(T));
        }

        private void Set(int idx, T t)
        {
            if (theList.Capacity < idx)
            {
                for (int i = theList.Capacity; i < idx; i++)
                {
                    theList.Add(default(T));
                }
            }

            theList[idx] = t;
        }

        private void RetrieveElements()
        {
            isElementsRetrieved = true;

            /* 
             * Elements can only exist in the DB
             * if this BOList element has been 
             * persisted. Hence if DBTag is null, 
             * there is nothing to load.
             */

            if (DBTag != null)
            {
                CollectionElementConverter cnv = new CollectionElementConverter(mt);

                foreach (IGenCollectionElement ce in Configuration.GenDB.AllElements(DBTag.EntityPOID))
                {
                    Set(ce.ElementIndex, (T)cnv.Translate(ce));
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

        /// <summary>
        /// Saves the elements to the database. This is handled 
        /// internally, why the public access modifier imposes
        /// an encapsulation problem. 
        /// </summary>
        public void SaveElementsToDB()
        {
            if (!isElementsRetrieved)
            {
                return; //Since nothing has been loaded, nothing needs to be saved.
            } 

            GenDB.DB.IGenericDatabase db = Configuration.GenDB;
            long thisPOID = this.DBTag.EntityPOID;
            db.ClearCollection(thisPOID);

            CollectionElementConverter cnv = new CollectionElementConverter(mt);

            Type elementType = typeof(T);

            for (int idx = 0; idx < theList.Count; idx++)
            {
                IGenCollectionElement ce = cnv.Translate(theList[idx]);
                ce.ElementIndex = idx;
                db.Save(ce, thisPOID, mt);
            }
        }

        private class CollectionElementConverter {
            MappingType mt;
            public CollectionElementConverter(MappingType mt) 
            { 
                this.mt = mt;
            }

            public object Translate(GenCollectionElement ce)
            {
                switch (mt)
                {
                    case MappingType.BOOL: return ce.BoolValue;
                    case MappingType.CHAR: return ce.CharValue;
                    case MappingType.DATETIME: return ce.DateTimeValue;
                    case MappingType.DOUBLE: return ce.DoubleValue;
                    case MappingType.REFERENCE: return GetObject(ce.RefValue);
                    case MappingType.STRING: return ce.StringValue;
                    default:
                        throw new Exception("MappingType not implemented in " + GetType().Name + " (" + mt + ")");
                }
            }

            private IBusinessObject GetObject(IBOReference reference)
            {
                if (reference.IsNullReference) { return null; }
                
                IBusinessObject ibo = IBOCache.Get (reference.EntityPOID);
                if (ibo != null) { return ibo; }

                IEntity e = Configuration.GenDB.GetEntity (reference.EntityPOID);

                IIBoToEntityTranslator trans = TypeSystem.GetTranslator(e.EntityType.EntityTypePOID);
                return trans.Translate(e);
            }

            public IGenCollectionElement Translate(object o)
            {
                if (o == null) { return null; }
                IGenCollectionElement res = new GenCollectionElement();
                switch (mt)
                {
                    case MappingType.BOOL:
                        res.BoolValue = Convert.ToBoolean (o);
                        break;
                    case MappingType.CHAR:
                        res.CharValue = Convert.ToChar (o);
                        break;
                    case MappingType.DATETIME:
                        res.DateTimeValue = Convert.ToDateTime (o);
                        break;
                    case MappingType.DOUBLE: 
                        res.DoubleValue = Convert.ToDouble (o);
                        break;
                    case MappingType.LONG:
                        res.LongValue = Convert.ToInt64(o);
                        break;
                    case MappingType.REFERENCE:
                        res.RefValue = GetReference(o);
                        break;
                    case MappingType.STRING:
                        res.StringValue = Convert.ToString(o);
                        break;
                    default:
                        throw new Exception("MappingType not implemented in " + GetType().Name + " (" + mt + ")");
                }
                return res;
            }

            private IBOReference GetReference(object o)
            {
                if (o == null) { return new IBOReference(true); }
                IBusinessObject ibo = (IBusinessObject)o;
                if (ibo.DBTag == null)
                {
                    IEntity e = Configuration.GenDB.NewEntity();
                    DBTag.AssignDBTagTo (ibo, e.EntityPOID);
                }
                return new IBOReference(ibo.DBTag.EntityPOID);
            }
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
