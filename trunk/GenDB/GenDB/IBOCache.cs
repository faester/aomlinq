using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GenDB.DB;
using System.Expressions;
using System.Query;

namespace GenDB
{

    /// <summary>
    /// Keeps track of objects that are both residing in the database and 
    /// in the application layer.  
    /// </summary>
    internal class IBOCache
    {
        /// <summary>
        /// Constructor with a parameter specifying which 
        /// datacontext to use for all other operations.
        /// </summary>
        /// <param name="dataContext"></param>
        internal IBOCache(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        DataContext dataContext;

        /// <summary>
        /// The number of comitted objects stored in the cache.
        /// </summary>
        internal int CommittedObjectsSize
        {
            get
            {
                return committedObjects.Count;
            }
        }

        /// <summary>
        /// The number of uncomitted objects stored in the cache.
        /// </summary>
        internal int UnCommittedObjectsSize
        {
            get { return uncommittedObjects.Count; }
        }

        /// <summary>
        /// Stores objects with weak references to allow garbage collection of 
        /// the cached objects.
        /// </summary>
        Dictionary<int, IBOCacheElement> committedObjects = new Dictionary<int, IBOCacheElement>();

        ///// <summary>
        ///// Stores regular object references. The object must not be gc'ed before 
        ///// it has been written to persistent storage.
        ///// </summary>
        Dictionary<int, IBusinessObject> uncommittedObjects = new Dictionary<int, IBusinessObject>();

        /// <summary>
        /// Adds an object to the dictionary of uncomitted objects.
        /// </summary>
        /// <param name="obj"></param>
        private void AddToCommitted(IBusinessObject obj)
        {
            IBOCacheElement ce = new IBOCacheElement(obj);
            committedObjects[obj.DBIdentity] = ce;
        }

        /// <summary>
        /// The total number of objects stored in the cache.
        /// </summary>
        public int Count
        {
            get { return CommittedObjectsSize + UnCommittedObjectsSize; }
        }

        /// <summary>
        /// Returns the business object identified 
        /// by the given id. Will return null if id
        /// is not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool TryGet(int entityPOID, out IBusinessObject obj)
        {
            IBOCacheElement wr;
            if (!committedObjects.TryGetValue(entityPOID, out wr))
            {
                if (!uncommittedObjects.TryGetValue(entityPOID, out obj))
                {
                    return false;
                }
            }
            else
            {
                obj = wr.Element;
            }
            return true;
        }

        /// <summary>
        /// Will null the regular references to the cached objects.
        /// Try to perform a garbage collection and let the DBTag destructor 
        /// remove irrelevant elements. Subsequently the strong references
        /// are set to point to the WeakReference's targets, to suppress garbage
        /// collection until next commit.
        /// </summary>
        private void TryGC()
        {
            // Set real references to null for all cached objects.
            IBOCacheElement[] ll = new IBOCacheElement[committedObjects.Count];
            int idx = 0;

            foreach (IBOCacheElement ce in committedObjects.Values)
            {
                ll[idx++] = ce;
                ce.ReleaseStrongReference();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            foreach (IBOCacheElement ce in ll)
            {
                if (ce.IsAlive)
                {
                    ce.ReEstablishStrongReference();
                }
                else
                {
                    int entityPOID = ce.EntityPOID;
                    committedObjects.Remove(entityPOID);
                    ce.Dispose();
                    Remove(entityPOID);
                }
            }
            ll = null;
        }


        /// <summary>
        /// When a SubmitChanges is called new objects 
        /// are stored indiscriminately. To ensure database consistency with the 
        /// application layer a submit will also cause the already stored objects 
        /// to be compared with their property state when the object was last 
        /// submitted to the databse. If a state change is found the object is 
        /// rewritten to the database.
        /// </summary>
        public void SubmitChanges()
        {
            CommitChangedCommitted();
            // Der kan være objekter i committed, der henviser til andre objekter i committed.
            CommitUncommitted();

            TryGC();
            
            dataContext.GenDB.CommitChanges();
        }

        /// <summary>
        /// Removes objects added to the cache since last commit. 
        /// When objects are retrieved from the database they are stored
        /// in the list of committed objects, so state changes in those
        /// objects will still be tracked, if submit is invoked later on.
        /// </summary>
        internal void RollbackTransaction()
        {
            uncommittedObjects.Clear();
            TryGC();
        }

        /// <summary>
        /// Commits all uncomitted objects to the database. 
        /// </summary>
        private void CommitUncommitted()
        {
            while (uncommittedObjects.Count > 0)
            {
                Dictionary<int, IBusinessObject> tmpUncomitted = uncommittedObjects;

                // Make a new uncomitted collection to allow clrtype2translator to add objects to the cache.
                uncommittedObjects = new Dictionary<int, IBusinessObject>();

                foreach (KeyValuePair<int,  IBusinessObject> kvp in tmpUncomitted)
                {
                    IBusinessObject ibo = kvp.Value;
                    IIBoToEntityTranslator trans = dataContext.Translators.GetTranslator(kvp.Value.GetType());
                    trans.SaveToDB(ibo);
                    AddToCommitted(ibo);
                }
            }
        }

        /// <summary>
        /// Commits objects that has been comitted once, if at 
        /// least one of its fields has changed since the last 
        /// commit.
        /// </summary>
        private void CommitChangedCommitted()
        {
            foreach (IBOCacheElement ce in committedObjects.Values)
            {
                if (ce.Element.DBIdentity.IsPersistent && ce.IsDirty)
                {
                    IBusinessObject ibo = ce.Element;
                    IIBoToEntityTranslator trans = dataContext.Translators.GetTranslator(ibo.GetType());
                    trans.SaveToDB(ibo);
                    ce.SetNotDirty();
                }
            }
        }

        /// <summary>
        /// Adds object to the cache and assigns a DBTag 
        /// at the same time.
        /// </summary>
        /// <param name="source">IBusinessObject to add</param>
        internal void Add(IBusinessObject ibo)
        {
            if (ibo.DBIdentity.IsPersistent)
            {
                throw new Exception("Was already set...");
            }

            Type t = ibo.GetType();

            CheckRegisterType(t);
            
            int entityPOID = ibo.DBIdentity.Value;

            if (entityPOID == 0)
            {
                entityPOID = dataContext.GenDB.NextEntityPOID;
            }

            ibo.DBIdentity = new DBIdentifier(entityPOID, true);

            uncommittedObjects.Add(entityPOID, ibo);
        }


        private void CheckRegisterType(Type t)
        {
            if (!dataContext.TypeSystem.IsTypeKnown(t))
            {
                dataContext.TypeSystem.RegisterType(t);
            }
        }
        

        /// <summary>
        /// When the db reads in a new object, it should be stored in the
        /// cache, but not be put in the UnCommittedObjects dictionary, since
        /// all objects in this list are persisted. 
        /// 
        /// We also assume that this method is only called from the db, why the 
        /// id part of the identity should be set before this method is invoked.
        /// </summary>
        /// <param name="element"></param>
        internal void AddFromDB(IBusinessObject ibo)
        {
            if (!ibo.DBIdentity.IsPersistent)
            {
                throw new Exception("Something wrong in DBIdentity class.");
            }

            // We need not check that type is registered here, since 
            // the object has been stored to the database earlier, and
            // the type is guaranteed to have been registered at that 
            // point.

            uncommittedObjects.Add(ibo.DBIdentity, ibo);
        }

        /// <summary>
        /// Removes the object identified 
        /// by the id from the cache. If object is 
        /// dirty its new state will be added to the database.
        /// (Ment to be invoked only by 
        /// the DBTag destructor)
        /// </summary>
        /// <param name="id"></param>
        internal void Remove(int id)
        {
            IBOCacheElement obj = null;
            IBusinessObject ibo = null;

            if (committedObjects.TryGetValue(id, out obj))
            {
                ibo = obj.Element;
                if (ibo != null)
                {
                    // Ensure that object is treated as non-persistent in later translations
                    // Since DBIdentity.Value is non-null, 
                    DBIdentifier newID = new DBIdentifier(ibo.DBIdentity.Value, false);
                    obj.Element.DBIdentity = newID;
                }
            }

            // Indicate that object is no longer under cache control
            if (uncommittedObjects.TryGetValue(id, out ibo))
            {
                // Ensure that object is treated as non-persistent in later translations
                DBIdentifier newID = new DBIdentifier(ibo.DBIdentity.Value, false);
                ibo.DBIdentity = newID;
                uncommittedObjects.Remove(id);
            }
        }
    }
}
