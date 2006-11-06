using System;
using System.Collections.Generic;
using System.Text;
using Persistence;
using AOM;

namespace Persistence
{
    /// <summary>
    /// Maintains relationship between in-memory objects and their IDs. 
    /// This is a problem as regards garbage collection, since no object 
    /// will ever be GC'ed when it is stored in the cache. This effectively 
    /// renders the DBTag's superfluous: The idea of the DBTag is to ensure
    /// call back when IBusinessObjects are finalized (the DBTag should 
    /// only be referenced here) since this will ensure the DBTag's 
    /// destructor is called. However, since no gc happens for the 
    /// parenting IBusinessObject, no GC will happen for the DBTag. 
    /// The structure is kept in the hope that we will manage to get 
    /// the garbage collector to neglect the object storage inside 
    /// the ObjectCache. (Perhabs using unsafe methods and some way to
    /// access the reference count of the objects stored in the cache. 
    /// This is a PENDING TASK!)
    /// </summary>
    public static class ObjectCache
    {
        /// <summary>
        /// Assign to private field in ObjectCache. 
        /// Will invoke the cleanup methods when 
        /// garbage collected. This happens only 
        /// on program shut down, since the field 
        /// is static.
        /// </summary>
        private class CleanUpObjectCache
        {

            ~CleanUpObjectCache()
            {
                ObjectCache.CleanupUnusedIds();
            }
        }

        static CleanUpObjectCache cleanup = new CleanUpObjectCache();  //See note for CleanUpObjectCache
        static Database dbinstance = Database.Instance;
        private const int ENTITY_IDS_TO_ALLOCATE = 100;

        //private static ObjectCache instance;
        private const string DUMMY_ENTITY_TYPE_NAME = "...dummytype"; //Used to get ids from DB. ()
        private static EntityType dummyType = null;

        private static Dictionary<long, object> id2obj;
        private static Dictionary<object, long> obj2id;
        private static LinkedList<long> unusedIds = new LinkedList<long>(); //List of IDS not in use by the DB. 

        static ObjectCache()
        {
            id2obj = new Dictionary<long, object>();
            obj2id = new Dictionary<object, long>();
            CreateDummeEntityType();
            dummyType = EntityType.GetType(DUMMY_ENTITY_TYPE_NAME);
        }

        /// <summary>
        /// Look up ID based on OBJECT. Be ware of autoboxing!!!!
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool HasObject(object obj)
        {
            return obj2id.ContainsKey(obj);
        }

        public static bool HasId(long id)
        {
            return id2obj.ContainsKey(id);
        }

        public static object GetObjectByID(long id)
        {
            return id2obj[id];
        }

        public static long GetIDByObject(object obj)
        {
            return obj2id[obj];
        }

        /// <summary>
        /// Should be internal. Namespaces Translation 
        /// and Persistence should be merged.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static void Store(object obj, long id)
        {
            obj2id[obj] = id;
            id2obj[id] = obj;
        }

        public static void RemoveObject(object obj)
        {
            id2obj.Remove(obj2id[obj]);
            obj2id.Remove(obj);
        }

        private static void RemoveByID(long id)
        {
            obj2id.Remove(id2obj[id]);
            id2obj.Remove(id);
        }

        public static long GetNewUnusedId()
        {
            long res;
            lock (unusedIds)
            {
                if (unusedIds.Count == 0)
                {
                    PopulateUnusedIds();
                }
                res = unusedIds.First.Value;
                unusedIds.RemoveFirst();
            }
            return res;
        }

        #region private stuff
        private static void CreateDummeEntityType()
        {
            if (!EntityType.EntityTypeExists(DUMMY_ENTITY_TYPE_NAME))
            {
                EntityType.CreateType(DUMMY_ENTITY_TYPE_NAME, null);
            }
        }

        private static void CleanupUnusedIds()
        {
            lock (unusedIds)
            {
                foreach (long unusedId in unusedIds)
                {
                    dbinstance.Delete(unusedId);
                }
                unusedIds.Clear();
            }
        }

        private static void PopulateUnusedIds()
        {
            for (int i = 0; i < ENTITY_IDS_TO_ALLOCATE; i++)
            {
                Entity e = dummyType.New();
                dbinstance.Store(e);
                unusedIds.AddLast(e.Id);
                e = null;
            }
        }
        #endregion

        #region Database encapsulation
        public static void StoreEntity(Entity e)
        {
            dbinstance.Store(e);
        }

        public static void StoreEntityType(EntityType et)
        {
            dbinstance.Store(et);
        }

        public static Entity RetrieveEntity(long id)
        {
            return dbinstance.Retrieve(id);
        }

        public static void Delete(DBTag tag)
        {
            dbinstance.Delete(tag);
            RemoveByID(tag.Id);
        }
        #endregion
    }
}
