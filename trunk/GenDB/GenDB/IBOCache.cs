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
        internal class CommittedObjectsOfType : IEnumerable<IBOCacheElement>, System.Collections.IEnumerable
        {
            int entityTypePOID;

            public int EntityTypePOID
            {
                get { return entityTypePOID; }
                private set { entityTypePOID = value; }
            }

            public CommittedObjectsOfType(int entityTypePOID)
            {
                EntityTypePOID = entityTypePOID;
            }

            Dictionary<int, IBOCacheElement> objs = new Dictionary<int, IBOCacheElement>();

            public IBOCacheElement this[int entityPOID] {
                get { return objs[entityPOID];}
                internal set { objs[entityPOID] = value; }
            }

            public int Count { get { return objs.Count; } }

            public void Remove(int entityPOID)
            {
                objs.Remove(entityPOID);
            }

            public bool TryGetValue(int entityPOID, out IBOCacheElement ce)
            {
                return objs.TryGetValue(entityPOID, out ce);
            }

            #region IEnumerable<IBOCacheElement> Members
            IEnumerator<IBOCacheElement> IEnumerable<IBOCacheElement>.GetEnumerator()
            {
               return objs.Values.GetEnumerator();
            }
            #endregion

            #region IEnumerable Members
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new Exception("Not implemented (IEnumerator)");
                //return this.GetEnumerator<IBOCacheElement>();
            }
            #endregion
        }

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
                int res = 0;
                foreach(CommittedObjectsOfType co in committedObjects.Values )
                {
                    res += co.Count;
                }
                return res;
            }
        }

        /// <summary>
        /// The number of uncomitted objects stored in the cache.
        /// </summary>
        internal int UnCommittedObjectsSize
        {
            get { 
                return uncommittedObjects.Count;
            }
        }

        /// <summary>
        /// Stores objects with weak references to allow garbage collection of 
        /// the cached objects.
        /// </summary>
        //Dictionary<long, IBOCacheElement> committedObjects = new Dictionary<long, IBOCacheElement>();

        Dictionary<int, CommittedObjectsOfType> committedObjects = new Dictionary<int, CommittedObjectsOfType>();

        ///// <summary>
        ///// Stores regular object references. The object must not be gc'ed before 
        ///// it has been written to persistent storage.
        ///// </summary>
        Dictionary<int, IBusinessObject> uncommittedObjects = new Dictionary<int, IBusinessObject>();

        Dictionary<int, int> entityToTypeMapping = new Dictionary<int, int>();

        /// <summary>
        /// Adds an object to the dictionary of uncomitted objects.
        /// </summary>
        /// <param name="obj"></param>
        private void AddToCommitted(int entityTypePOID, IBusinessObject obj)
        {
            IBOCacheElement wr = new IBOCacheElement(obj);
            committedObjects[entityTypePOID][obj.DBIdentity] = wr;
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
            int entityTypePOID;

            if (!entityToTypeMapping.TryGetValue(entityPOID, out entityTypePOID))
            {
                obj = null;
                return false;
            }

            IBOCacheElement wr;
            if (!committedObjects[entityTypePOID].TryGetValue(entityPOID, out wr))
            {
                if (!uncommittedObjects.TryGetValue(entityPOID, out obj))
                {
                    return false;
                }
            }
            else
            {
                if (!wr.IsAlive)
                {
                    committedObjects[entityTypePOID].Remove(entityPOID);
                    entityToTypeMapping.Remove(entityPOID);
                    obj = null;
                    return false;
                }
                else
                {
                    obj = wr.Target;
                }
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
            foreach (CommittedObjectsOfType co in committedObjects.Values)
            {
                IBOCacheElement[] ll = new IBOCacheElement[co.Count];
                int idx = 0;

                foreach (IBOCacheElement ce in co)
                {
                    ll[idx++] = ce;
                    ce.Original = null;
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
             
                foreach (IBOCacheElement ce in ll)
                {
                    if (ce.IsAlive)
                    {
                        ce.Original = ce.Target;
                    }
                    else
                    {
                        int entityPOID = ce.EntityPOID;
                        int entityTypePOID = entityToTypeMapping[entityPOID];
                        committedObjects[entityTypePOID].Remove(entityPOID);
                        Remove(entityPOID);
                    }
                }
                ll = null;
            }
        }


        internal void PrepareForType(int entityTypePOID)
        {
            committedObjects[entityTypePOID] = new CommittedObjectsOfType(entityTypePOID);
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
            CommitUncommitted();
            CommitChangedCommitted();
            // Der kan være objekter i committed, der henviser til andre objekter i committed.

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
            foreach(int i in uncommittedObjects.Keys)
            {
                entityToTypeMapping.Remove(i);
            }
            uncommittedObjects.Clear();
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
                    int entityTypePOID = entityToTypeMapping[kvp.Key];
                    IBusinessObject ibo = kvp.Value;
                    IIBoToEntityTranslator trans = dataContext.Translators.GetTranslator(entityTypePOID);
                    trans.SaveToDB(ibo);
                    AddToCommitted(entityTypePOID, ibo);
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
            foreach (CommittedObjectsOfType co in committedObjects.Values)
            {
                foreach (IBOCacheElement ce in co)
                {
                    if (ce.Original.DBIdentity.IsPersistent && ce.IsDirty)
                    {
                        if (ce.IsAlive)
                        {
                            IBusinessObject ibo = ce.Original;
                            IIBoToEntityTranslator trans = dataContext.Translators.GetTranslator(ibo.GetType());
                            trans.SaveToDB(ibo);

                            ce.ClearDirtyBit();
                        }
                        else
                        {
                            //TODO: Should never happen, but need proper testing....
                            throw new Exception("Object reclaimed before if was flushed to the DB.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds object to the cache and assigns a DBTag 
        /// at the same time.
        /// </summary>
        /// <param name="clone"></param>
        /// <param name="knudBoergesBalsam"></param>
        internal void Add(IBusinessObject ibo, int entityPOID)
        {
            if (ibo.DBIdentity.IsPersistent)
            {
                throw new Exception("Was already set...");
            }

            int entityTypePOID = dataContext.TypeSystem.GetEntityType(ibo.GetType()).EntityTypePOID;

            entityToTypeMapping[entityPOID] = entityTypePOID;

            ibo.DBIdentity = new DBIdentifier(entityPOID, true);

            uncommittedObjects.Add(entityPOID, ibo);
        }

        

        /// <summary>
        /// When the db reads in a new object, it should be stored in the
        /// cache, but not be put in the UnCommittedObjects dictionary, since
        /// all objects in this list are persisted. 
        /// 
        /// We also assume that this method is only called from the db, why the 
        /// id part of the identity should be set before this method is invoked.
        /// </summary>
        /// <param name="ibo"></param>
        internal void AddFromDB(IBusinessObject ibo)
        {
            if (!ibo.DBIdentity.IsPersistent)
            {
                throw new Exception("Something wrong in DBIdentity class.");
            }
            int entityTypePOID = dataContext.TypeSystem.GetEntityType(ibo.GetType()).EntityTypePOID;

            entityToTypeMapping[ibo.DBIdentity] = entityTypePOID;

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
            int entityTypePOID;

            if (!entityToTypeMapping.TryGetValue(id, out entityTypePOID))
            {
                return; 
            }
            
            if (committedObjects[entityTypePOID].TryGetValue(id, out obj))
            {
                ibo = obj.Original;
                if (ibo != null)
                {
                    DBIdentifier newID = new DBIdentifier(ibo.DBIdentity.Value, false);
                    obj.Original.DBIdentity = newID;
                }
            }

            // Indicate that object is no longer under cache control
            if (uncommittedObjects.TryGetValue(id, out ibo))
            {
                DBIdentifier newID = new DBIdentifier(ibo.DBIdentity.Value, false);
                ibo.DBIdentity = newID;
                uncommittedObjects.Remove(id);
                entityToTypeMapping.Remove(id);

            }
        }
    }
}
