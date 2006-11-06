//#define CACHE_ENTITIES
using System;
using System.Collections.Generic;
using System.Text;
using AOM;
using System.Data.SqlClient;
using System.Data;

namespace Persistence
{
    /// <summary>
    /// Singleton, lazy initialization.
    /// TODO: Classes are cached based on their EntityPOID. Since this 
    /// is identical to all instances within a hierarchy, the most specialized
    /// Entity is stored. It contains pointers to all super instances, and it will
    /// work in all cases, where no fields are shadowing each other. But we need to 
    /// make a work-around to make it possible to retrieve Entities based on their 
    /// type from the cache.
    /// </summary>
    internal sealed partial class Database
    {
        private const string CNN_STRING = "server=.;database=tests;uid=aom;pwd=aomuser";
        static Database instance;
#if CACHE_ENTITIES
        static Dictionary<long, Entity> loadedEntities = new Dictionary<long, Entity>();
#endif
        
        private Database() { /* empty */ }

        /// <summary>
        /// Creates an instance of the DB. For now the 
        /// connection string is defined in AOMConfig, 
        /// which is rather ugly.
        /// </summary>
        internal static Database Instance
        {
            get
            {
                if (instance == null)
                {
                    EntityTypeLoader.Load();
                    instance = new Database(CNN_STRING);
                }
                return instance;
            }
        }

        #region Fields

        object mutex = new object();

        //DataContext myDataContext;

        /// <summary>
        /// Wrapper used to store value fields.
        /// </summary>
        private StoreValueExecuter storeValueExecuter;

        /// <summary>
        /// Wrapper used to store EntityType ojects
        /// </summary>
        private StoreEntityTypeExecuter storeEntityTypeExecuter;

        /// <summary>
        /// Wrapper used to store Entity objects
        /// </summary>
        private StoreEntityExecuter storeEntityExecuter;

        /// <summary>
        /// Wrapper used to store Property objects
        /// </summary>
        private StorePropertyExecuter storePropertyExecuter;

        /// <summary>
        /// Wrapper used to store 
        /// </summary>
        private StorePropertyTypeExecuter storePropertyTypeExecuter;

        /// <summary>
        /// Wrapper used to store PropertyType objects.
        /// </summary>
        private StoreInheritanceExecuter storeInheritanceExecuter;

        /// <summary>
        /// Connection used throughout this objects lifetime
        /// to connect to the db. 
        /// </summary>
        internal SqlConnection cnn;

        /// <summary>
        /// Transaction of the current operation. 
        /// One transaction does in this case mean <i>no thread safety</i>!
        /// </summary>
        private SqlTransaction trans = null;

        #endregion

        #region Constructors and initializer
        private Database(string cnnStr)
        {
            InitConnection();
            InitCommands();
        }

        private void InitConnection()
        {
            cnn = new SqlConnection(AOMConfig.CNN_STRING);
            cnn.Open();
        }

        private void InitCommands()
        {
            storeValueExecuter = new StoreValueExecuter(cnn);
            storeEntityTypeExecuter = new StoreEntityTypeExecuter(cnn);
            storeEntityExecuter = new StoreEntityExecuter(cnn);
            storePropertyExecuter = new StorePropertyExecuter(cnn);
            storePropertyTypeExecuter = new StorePropertyTypeExecuter(cnn);
            storeInheritanceExecuter = new StoreInheritanceExecuter(cnn);
        }
        #endregion

        /// <summary>
        /// Stores the <pre>EntityType</pre> object given. Will create 
        /// a new record, if the <pre>EntityType</pre> is not already in 
        /// the db. Otherwise the database record with the specified id 
        /// will be updated to match the values of the <pre>EntityType</pre>
        /// given.
        /// <para>
        /// Will also store/update super entity types, if any exist. Likewise any 
        /// property and property types will be stored. 
        /// </para>
        /// </summary>
        /// <param name="et"></param>
        public void Store(EntityType et)
        {
            long oldID = et.Id;
            bool wasPersistent = et.IsPersistent;
            lock (mutex)
            {
                //Wrap in new transaction
                trans = cnn.BeginTransaction(IsolationLevel.Serializable);
                try {
                SetTransaction();
                //Call real store method
                _Store(et);
                }
                catch (SqlException ex)
                {
                    trans.Rollback();
                    et.Id = oldID;
                    et.IsPersistent = wasPersistent;
                    Console.WriteLine(ex.StackTrace);
                    throw ex;
                }
                catch (IDChangeAfterCommitException ice)
                {
                    trans.Rollback();
                    //et.Id = oldID;
                    et.IsPersistent = false;
                    Console.WriteLine(ice.StackTrace );
                    throw ice;
                }

                trans.Commit();
            }
        }

        private void SetTransaction()
        {
            storeValueExecuter.Transaction = trans;
            storeEntityTypeExecuter.Transaction = trans;
            storeEntityExecuter.Transaction = trans;
            storePropertyExecuter.Transaction = trans;
            storePropertyTypeExecuter.Transaction = trans;
            storeInheritanceExecuter.Transaction = trans;
        }



        /// <summary>
        /// Stores an entity and wrap the store request in a transaction.
        /// </summary>
        /// <param name="e"></param>
        public void Store(Entity e)
        {
            long oldID = e.Id;
            bool wasPersistent = e.IsPersistent;
            lock (mutex)
            {
                trans = cnn.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    SetTransaction();
                    _Store(e, null);
                }
                catch (SqlException ex)
                {
                    trans.Rollback();
                    e.Id = oldID;
                    e.IsPersistent = wasPersistent;
                    Console.WriteLine(ex.StackTrace);
                    throw ex;
                }
                catch (IDChangeAfterCommitException ice)
                {
                    trans.Rollback();
                    //e.Id = oldID;
                    e.IsPersistent = false;
                    Console.WriteLine(ice.StackTrace );
                    throw ice;
                }                trans.Commit();
#if CACHE_ENTITIES
                /* Store entity in the cache.
                 * Test if the entity object has changed, 
                 * since this might indicate problems.
                 */
                if (loadedEntities.ContainsKey(e.Id))
                {
                    System.Diagnostics.Debug.Assert(Object.ReferenceEquals(e, loadedEntities[e.Id]));
                }
                loadedEntities[e.Id] = e;
#endif
            }
        }


        #region private methods
        /// <summary>
        /// The real storage logic for storing EntityType objects
        /// </summary>
        /// <param name="et"></param>
        private void _Store(EntityType et)
        {
            //Check if et has already been committed. 
            if (et.IsPersistent) { return; }

            //Use executer to store values. Assumes IDs are set correct if neccessary.
            storeEntityTypeExecuter.Store(et);

            //Ensure that each property of the EntityType is stored
            foreach (Property pt in et.Properties)
            {
                _Store(pt, et);
            } // foreach

            //If a super type exists, it should be stored as well.
            if (et.SuperType != null)
            {
                _Store(et.SuperType);
                //And we also need to make the inheritance structure persistent.
                storeInheritanceExecuter.Store(et.SuperType, et);
            } // if 
        } //method


        /// <summary>
        /// Logic of storing entities. If <paramref name="e"/> 
        /// is stored as part of an object spanning several levels
        /// of inheritance <paramref name="subEntity"/> indicates 
        /// the nearest child. Entity objects are stored button up
        /// partly to ensure that the <pre>EntityGroupPOID</pre> field
        /// in the database is set correctly. This field is used to 
        /// retrieve all parts of an object with associated field
        /// values.
        /// </summary>
        /// <param name="e">Entity object to store</param>
        /// <param name="subEntity">The direct child of the Entity object to store. 
        /// Use null if this is the deepest entity in the object.
        /// </param>
        private void _Store(Entity e, Entity subEntity)
        {
            //Check if already stored
            if (e.IsPersistent) { return; }
            //Check if type is known. EntityType must be stored prior to the Entity objects instantiating et.
            if (!e.Type.IsPersistent) { _Store(e.Type); }

            /**
             * Objects with inheritance are stored distributed on 
             * several Entity instances. EntityGroup is the ID of
             * the deepest child in the inheritance tree and is 
             * used to glue the object together.
             */
            storeEntityExecuter.Store(e, subEntity);

            /*
             * Recursively descent to all super entities 
             * until the root is reached.
             */
            if (e.EntityBase != null) { _Store(e.EntityBase, e); }

            /*
             * StoreEntity value of all properties.
             */
            foreach (Property p in e.Type.Properties)
            {
                storeValueExecuter.Store(e, p);
            }
        }


        /// <summary>
        /// StoreEntity a <pre>Property</pre>
        /// </summary>
        /// <param name="p">Property to store</param>
        /// <param name="et">EntityType that <paramref name="p"/> belongs to.</param>
        private void _Store(Property p, EntityType et)
        {
            if (p.IsPersistent) { return; }
            _Store(p.Type);

            storePropertyExecuter.Store(p, et);
        }

        /// <summary>
        /// StoreEntity a property type.
        /// </summary>
        /// <param name="pt"></param>
        private void _Store(PropertyType pt)
        {
            if (pt.IsPersistent) { return; }

            storePropertyTypeExecuter.Store(pt);
        }

        /// <summary>
        /// Removes the Entity identified by the DBTag 
        /// completely. 
        /// </summary>
        /// <param name="tag"></param>
        public void Delete (DBTag tag)
        {
            if (tag == null) throw new NullReferenceException ("dbtag");
            Delete (tag.Id);
        }

        public void Delete (long id)
        {
#if CACHE_ENTITIES
            loadedEntities.Remove (tag.Id);
#endif
            SqlCommand deleter = new SqlCommand ("DELETE FROM Entity WHERE EntityPOID = " + id, cnn);
            deleter.ExecuteNonQuery();
            deleter.CommandText = "DELETE FROM value WHERE EntityPOID = " + id;
            deleter.ExecuteNonQuery();
            deleter = null;
        }
        #endregion

        #region Retrieve
        /// <summary>
        /// Retrieves an Entity based on the ID.
        /// Loaded entities are cached and returned 
        /// based on the ID.
        /// TODO: Needs efficient implementation!
        /// </summary>
        /// <param name="entityPOID">EntityPOID of Entity to load. Must exist!</param>
        /// <returns></returns>
        public Entity Retrieve(long entityPOID)
        {
            lock(mutex)
            {
                return _Retrieve(entityPOID);
            }
        }

        private Entity _Retrieve(long entityPOID)
        {
#if CACHE_ENTITIES
            if (loadedEntities == null)
            {
                throw new Exception ("Internal error in database!");
            }
#endif
            //Check if cache contains the Entity already. 
#if CACHE_ENTITIES
            if (loadedEntities.ContainsKey(entityPOID))
            {
                return loadedEntities[entityPOID];
            }
#endif

            Entity res = null;
            SqlCommand cmd = new SqlCommand(
                " SELECT  "
                + " v.value,  "
                + " v.PropertyPOID propertyPOID,  "
                + " v.EntityPOID,  "
                + " p.Name,  "
                + " et.EntityTypePOID, "
                + " et.Name  "
                + " FROM  "
                + " Value v INNER JOIN Entity e "
                + "	ON e.EntityPOID = v.EntityPOID  "
                + " INNER JOIN EntityType et "
                + "	ON e.EntityTypePOID = et.EntityTypePOID  "
                + " INNER JOIN Property p "
                + "	ON p.EntityTypePOID = e.EntityTypePOID   AND p.PropertyPOID = v.PropertyPOID "
                + " WHERE v.EntityPOID = " + entityPOID.ToString()
                + " ORDER BY e.EntityTypePOID "
                    , cnn);
            SqlDataReader r = cmd.ExecuteReader();

            // The ordering of the retrieved objects is not defined, so the
            // parts are stored here and will be assembled when everything is loaded.
            Dictionary<string, Entity> components = new Dictionary<string, Entity>();
            LinkedList<Entity> componentlist = new LinkedList<Entity>();
            long currentEntityTypePOID = EntityType.UNDEFINED_ID;
            Entity currentEntity = null;
            EntityType currentEntityType = null;

            while (r.Read())
            {
                string value = r[0].ToString();
                long propertyPOID = long.Parse(r[1].ToString());
                long centityPOID = long.Parse(r[2].ToString());
                string propertyName = r[3].ToString();
                //string propertyTypeName = r[4].ToString();
                long entityTypePOID = long.Parse(r[4].ToString());
                string entityTypeName = r[5].ToString();
                if (entityTypePOID != currentEntityTypePOID)
                {
                    if (currentEntity != null)
                    {
                        components[currentEntity.Type.Name] = currentEntity;
                        componentlist.AddLast(currentEntity);
                    }
                    currentEntityType = EntityType.GetType(entityTypeName);
                    currentEntity = currentEntityType.New();
                    currentEntity.Id = centityPOID;
                    currentEntityTypePOID = entityTypePOID;
                }
                currentEntity.SetProperty(propertyName, value);
            }

            components[currentEntity.Type.Name] = currentEntity;
            componentlist.AddLast(currentEntity);

            foreach (Entity e in componentlist)
            {
                if (e.Type.SuperType != null)
                {
                    string superName = e.Type.SuperType.Name;
                    Entity eb = null;
                    if (components.TryGetValue(superName, out eb))
                    {
                        e.EntityBase = eb;
                    }
                    else
                    {
                        e.EntityBase = EntityType.GetType(superName).New();
                    }
                    /* Remove every part that is a super entity of some 
                     * other entity in the set. Should leave the Entity
                     * with the lowest position in the hierarchy.
                     */
                    components.Remove(superName);
                }
            }

            if (components.Count != 1)
            {
                throw new Exception("All elements of the Entity was not properly constructed!");
            }

            foreach (KeyValuePair<string, Entity> kvp in components)
            {
                res = kvp.Value;
            }

            if (!r.IsClosed) { r.Close(); }
#if CACHE_ENTITIES
            loadedEntities[res.Id] = res;
#endif
            return res;
        }

        public LinkedList<Entity> RetrieveAll(string entityTypeName)
        {
            lock (mutex)
            {
                return _RetrieveAll(entityTypeName);
            }
        }


        private LinkedList<Entity> _RetrieveAll(string entityTypeName)
        {
            SqlConnection cnn2 = new SqlConnection(AOMConfig.CNN_STRING);
            cnn2.Open();
            SqlCommand cmd = new SqlCommand("SELECT DISTINCT EntityPOID FROM "
                                            + " Entity INNER JOIN EntityType "
                                            + "   ON Entity.EntityTypePOID = EntityType.EntityTypePOID "
                                            + " WHERE EntityType.Name = '" + entityTypeName + "'",
                                            cnn2);
            SqlDataReader reader = cmd.ExecuteReader();

            LinkedList<Entity> res = new LinkedList<Entity>();

            while (reader.Read())
            {
                long entityPOID = long.Parse(reader[0].ToString());
                res.AddLast(Retrieve(entityPOID));

            }
            if (!reader.IsClosed) { reader.Close(); }

            return res;
        }
        #endregion

    }
}
