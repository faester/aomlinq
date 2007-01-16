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

        int generation = 0;

        private static IBOCache instance = null;

        /// <summary>
        /// Returns the singleton instance of the IBOCache.
        /// It is mandatory to run IBOCache.Init(DataContext) before 
        /// the first access to the instance, in order to set the 
        /// DataContext for the IBOCache.
        /// </summary>
        public static IBOCache Instance 
        {
            get { return instance; }
        }

        public static void Init(DataContext dataContext)
        {
            instance = new IBOCache(dataContext);
        }

        long retrieved = 0;

        private IBOCache(DataContext dataContext)
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
            IBOCacheElement wr = new IBOCacheElement(obj, generation);
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
        public IBusinessObject Get(long id)
        {
            IBOCacheElement wr;
            IBusinessObject result;
            if (!committedObjects.TryGetValue(id, out wr))
            {
                if (!uncommittedObjects.TryGetValue(id, out result))
                {
                    return null;
                }
            }
            else
            {
                if (!wr.IsAlive) { throw new Exception("Internal error in cache: Object has been reclaimed by garbagecollector, but was requested from cache."); }
                result = wr.Target;
            }
            retrieved++;
            return result;
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
#if DEBUG
            Console.WriteLine("DEBUG INFORMATION FROM IBOCache.FlushToDB():");
            Stopwatch stp = new Stopwatch();
            
            stp.Start();
#endif
            CommitChangedCommitted();
#if DEBUG
            stp.Stop();
            Console.WriteLine("\tCommitUncomitted took: {0}", stp.Elapsed);
            stp.Reset();

            stp.Start();
#endif
            CommitUncommitted();
            TryGC();
#if DEBUG
            stp.Stop();
            Console.WriteLine("\tCommitChangedComitted took: {0}", stp.Elapsed);

            stp.Reset();
            stp.Start();
#endif
            generation++;
            dataContext.GenDB.CommitChanges();
#if DEBUG
            stp.Stop();
            Console.WriteLine("\tConfiguration.GenDB.CommitChanges took: {0}", stp.Elapsed);
#endif
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
                if (ce.IsDirty)
                {
                    if (ce.IsAlive)
                    {
                        IBusinessObject ibo = ce.Target;
                        IIBoToEntityTranslator trans = dataContext.Translators.GetTranslator(ibo.GetType());
                        trans.SaveToDB(dataContext.GenDB, ibo);
                        //IEntity e = trans.PickCorrectElement(res);
                        //DataContext.GenDB.Save(e);
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
        internal void Add(IBusinessObject ibo, long entityPOID)
        {
            //DBTag dbTag = new DBTag( /* this, */ knudBoergesBalsam);
            if (ibo.DBIdentity.IsPersistent)
            {
                throw new Exception ("Was already set...");
            }

            ibo.DBIdentity = new DBIdentifier(entityPOID);

            uncommittedObjects.Add(entityPOID, ibo);
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
            //TODO: Need to make some kind of commit possible here.
            committedObjects.Remove(id);
            uncommittedObjects.Remove(id);
        }
    }
}
