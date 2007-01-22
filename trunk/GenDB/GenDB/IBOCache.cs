using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GenDB.DB;
#if DEBUG
using System.Diagnostics;
#endif

namespace GenDB
{
    internal class IBOCache
    {
        int MAX_OLD_OBJECTS_TO_KEEP = 1000;

        //private static IBOCache instance = null;

        ///// <summary>
        ///// Returns the singleton instance of the IBOCache.
        ///// It is mandatory to run IBOCache.Init(DataContext) before 
        ///// the first access to the instance, in order to set the 
        ///// DataContext for the IBOCache.
        ///// </summary>
        //public static IBOCache Instance 
        //{
        //    get { return instance; }
        //}

        //public static void Init(DataContext dataContext)
        //{
        //    instance = new IBOCache(dataContext);
        //}

        long retrieved = 0;

        internal IBOCache(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        DataContext dataContext;

        public long Retrieved
        {
            get { return retrieved; }
        }

        internal int CommittedObjectsSize
        {
            get { 
                return committedObjects.Count;
            }
        }

        internal int UnCommittedObjectsSize
        {
            get { return uncommittedObjects.Count; }
        }

        /// <summary>
        /// Stores objects with weak references to allow garbage collection of 
        /// the cached objects.
        /// </summary>
        Dictionary<long, IBOCacheElement> committedObjects = new Dictionary<long, IBOCacheElement>();

        ///// <summary>
        ///// Stores regular object references. The object must not be gc'ed before 
        ///// it has been written to persistent storage.
        ///// </summary>
        Dictionary<long, IBusinessObject> uncommittedObjects = new Dictionary<long, IBusinessObject>();

        Dictionary<long, IBusinessObject> oldObjects = new Dictionary<long, IBusinessObject>();

        LinkedList<IBusinessObject> oldObjectsHotlist = new LinkedList<IBusinessObject>();
 
        private void AddToOldObjects()
        {

        }

        private void AddToCommitted(IBusinessObject obj)
        {
            IBOCacheElement wr = new IBOCacheElement(obj);
            committedObjects[obj.DBIdentity] = wr;
        }

        public int Count
        {
            get { return committedObjects.Count; }
        }

        /// <summary>
        /// Returns the business object identified 
        /// by the given id. Will return null if id
        /// is not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool TryGet(long id, out IBusinessObject obj)
        {
            IBOCacheElement wr;
            if (!committedObjects.TryGetValue(id, out wr))
            {
                if (!uncommittedObjects.TryGetValue(id, out obj))
                {
                    return false;
                }
            }
            else
            {
                if (!wr.IsAlive) { 
                    committedObjects.Remove(id);
                    obj = null;
                    return false;
                }
                else
                {
                    obj = wr.Target;
                }
            }
            retrieved++;
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
#if DEBUG
            Console.WriteLine("Committed objects contains {0} elements", committedObjects.Count);
            Console.WriteLine("IBOCache contains {0} committed objects.", CommittedObjectsSize);
            int removeCount = 0;
#endif
            IBOCacheElement[] ll = new IBOCacheElement[committedObjects.Count];
            committedObjects.Values.CopyTo(ll, 0);
            foreach (IBOCacheElement ce in ll)
            {
                ce.Original = null;
            }
            GC.Collect();
           
            foreach (IBOCacheElement ce in ll)
            {
                if (ce.IsAlive)
                {
                    ce.Original = ce.Target;
                }
                else
                {
#if DEBUG
                    removeCount++;
#endif
                    committedObjects.Remove(ce.EntityPOID);
                    Remove(ce.EntityPOID);
                }
            }
            ll = null;
#if DEBUG
            Console.WriteLine("IBOCache removed {0} objects.", removeCount);
            Console.WriteLine("Committed objects now contains {0} elements", committedObjects.Count);
#endif
        }

        public void FlushToDB()
        {
            CommitUncommitted();
            CommitChangedCommitted();
            // Der kan være objekter i committed, der henviser til andre objekter i committed.

            TryGC();

            dataContext.GenDB.CommitChanges();
        }

        private void CommitUncommitted()
        {
            while (uncommittedObjects.Count > 0)
            {
                Dictionary<long, IBusinessObject> tmpUncomitted = uncommittedObjects;

                // Make a new uncomitted collection to allow clrtype2translator to add objects to the cache.
                uncommittedObjects = new Dictionary<long, IBusinessObject>();

                foreach (IBusinessObject ibo in tmpUncomitted.Values)
                {
                    IIBoToEntityTranslator trans = dataContext.Translators.GetTranslator(ibo.GetType());
                    trans.SaveToDB(dataContext.GenDB, ibo);
                    AddToCommitted(ibo);
                }
            }
        }

        private void CommitChangedCommitted()
        {
            foreach (IBOCacheElement ce in committedObjects.Values)
            {
                if (ce.Original.DBIdentity.IsPersistent && ce.IsDirty)
                {
                    if (ce.IsAlive)
                    {
                        IBusinessObject ibo = ce.Original;
                        IIBoToEntityTranslator trans = dataContext.Translators.GetTranslator(ibo.GetType());
                        trans.SaveToDB(dataContext.GenDB, ibo);

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

        /// <summary>
        /// Adds object to the cache and assigns a DBTag 
        /// at the same time.
        /// </summary>
        /// <param name="res"></param>
        /// <param name="knudBoergesBalsam"></param>
        internal void Add(IBusinessObject ibo, int entityPOID)
        {
            if (ibo.DBIdentity.IsPersistent)
            {
                throw new Exception ("Was already set...");
            }

            ibo.DBIdentity = new DBIdentifier(entityPOID, true);

            uncommittedObjects.Add(entityPOID, ibo);
        }


        /// <summary>
        /// When the db reads in a new object, it should be stored in the
        /// cache, but not be put in the UnCommittedObjects dictionary, since
        /// all changes are persisted. 
        /// 
        /// We also assume that this method is only called from the db, why the 
        /// id part of the identity should be set correctly.
        /// </summary>
        /// <param name="ibo"></param>
        internal void AddFromDB(IBusinessObject ibo)
        {
            if (!ibo.DBIdentity.IsPersistent)
            {
                throw new Exception("Something wrong in DBIdentity class.");
            }
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
        internal void Remove(long id)
        {
            IBOCacheElement obj = null;
            if (committedObjects.TryGetValue(id, out obj))
            {
                IBusinessObject ibo = obj.Original;
                DBIdentifier newID = new DBIdentifier (ibo.DBIdentity.Value, false);
                obj.Original.DBIdentity = newID;
                if (obj.Original.DBIdentity.IsPersistent)
                {
                    throw new Exception("Internal error in DBIdentity");
                }
            }

            //committedObjects.Remove(id);
            uncommittedObjects.Remove(id);
        }
    }
}
