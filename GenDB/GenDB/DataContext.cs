using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;
using System.Data.SqlClient;

namespace GenDB
{
    /// <summary>
    /// The main entry point to the persistence framework. 
    /// Use the datacontext to set database name and obtain 
    /// new instances of Table&lt;T&gt;-objects.
    /// </summary>
    public class DataContext
    {
        static DataContext instance = new DataContext();

        bool isInitialized = false;

        public bool IsInitialized
        {
            get { return isInitialized; }
        }

        /// <summary>
        /// Returns a table capeable of holding 
        /// objects of type T. First call to this method 
        /// will initilize the DataContext.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Table<T> GetTable<T> ()
            where T : IBusinessObject, new()
        {
            if (!isInitialized)
            {
                Init();
            }
            return new Table<T>(GenDB, Translators, TypeSystem, IBOCache);
        }


        /// <summary>
        /// The instance of the DataContext. 
        /// </summary>
        public static DataContext Instance
        {
            get { return DataContext.instance; }
        }

        TranslatorSet translators;

        internal TranslatorSet Translators
        {
            get { return translators; }
        }

        TypeSystem typeSystem;

        internal TypeSystem TypeSystem
        {
            get { return typeSystem; }
        }

        IGenericDatabase genDB;

        internal IGenericDatabase GenDB
        {
            get { return genDB; }
        }

        IBOCache iboCache;

        internal IBOCache IBOCache
        {
            get { return iboCache; }
        }

        internal DataContext()
        {
            TranslatorSet.Init(this);
            translators = TranslatorSet.Instance;
            genDB = new MsSql2005DB (this);
        }


        /// <summary>
        /// Initializes DataContext. This implies that class descriptions 
        /// are loaded internally and the entire framework is prepared for 
        /// usage. Prior to calling this method it should be ensured that 
        /// the underlying database has been created etc. 
        /// </summary>
        public void Init()
        {
            if (isInitialized)
            {
                throw new Exception("Database already up running.");
            }
            iboCache = new IBOCache(this);

            TypeSystem.Init(this);
            typeSystem = TypeSystem.Instance;
            typeSystem.Init();
             
            
            isInitialized = true;
        }

        private int dbBatchSize = 500;

        /// <summary>
        /// Attempts to build database. All changes will be lost. 
        /// Cache will be emptied without committing. Should only 
        /// be used as part of program initialization before any-
        /// thing has been added to the database. Must only be called 
        /// when DataContext is still uninitialized.
        /// </summary>
        public void DeleteDatabase()
        {
            if (isInitialized)
            {
                throw new Exception("Database already up running.");
            }
            genDB.DeleteDatabase();
        }


        /// <summary>
        /// Attempts to build database. All changes will be lost. 
        /// Cache will be emptied. Should only be used as part 
        /// of program initialization. Must only be called 
        /// when DataContext is still uninitialized.
        /// </summary>
        public void CreateDatabase()
        {
            if (isInitialized)
            {
                throw new Exception("Database already up running.");
            }
            genDB.CreateDatabase();
        }

        /// <summary>
        /// Returns true if database exists. 
        /// </summary>
        /// <returns></returns>
        public bool DatabaseExists()
        {
            return genDB.DatabaseExists();
        }

        /// <summary>
        /// When the internal database sends commands (inserts/deletes/updates)
        /// to the database, it will concatenate a number of query strings and 
        /// send them as one batch. This number represents the batch size.
        /// Note that changes will only take effect for future database operations.
        /// The value should probably not be changed by the user. 
        /// </summary>
        public int DbBatchSize
        {
            get { return dbBatchSize; }
            set { dbBatchSize = value; }
        }

        private int commandTimeout = 0;

        /// <summary>
        /// Sets the TimeOut of commands created internally in the framework. 
        /// It will probably be a good idea to leave this unchanged.
        /// </summary>
        public int CommandTimeout
        {
            get { return commandTimeout; }
            set { commandTimeout = value; }
        }

        string connectStringWithDBName = "server=(local);database=generic;Integrated Security=SSPI;connection timeout=0";


        /// <summary>
        /// Returns a closed connection to the database.
        /// </summary>
        /// <returns></returns>
        internal SqlConnection CreateDBConnection()
        {
            return new SqlConnection(connectStringWithDBName);
        }

        /// <summary>
        /// Will create a connection to the server. The connection will not be opened, and no 
        /// database name is set.
        /// </summary>
        /// <returns></returns>
        internal SqlConnection CreateServerConnection()
        {
            return new SqlConnection(connectStringWithoutDBName);
        }

        string connectStringWithoutDBName = "server=(local);Integrated Security=SSPI;connection timeout=0";

        string dbname = "generic";

        /// <summary>
        /// Sets the database name. An exception will be thrown if 
        /// the name is changed after DataContext has been initialized.
        /// </summary>
        public string DatabaseName 
        {
            get { return dbname; }
            set {
                if (IsInitialized) { throw new Exception("Database name can not be changed when the datacontext has been initialized."); }
                dbname = value; 
                connectStringWithDBName = connectStringWithoutDBName + ";database=" + value;
            }
        }


        /// <summary>
        /// Will submit newly added as well as changed object to the database.
        /// </summary>
        public void SubmitChanges()
        {
            iboCache.SubmitChanges();
        }

        /// <summary>
        /// All objects added since last commit will be removed from the cache and thus
        /// not added to the database. (Unless they are referenced by some other persisted object.)
        /// </summary>
        public void RollbackTransaction()
        {
            iboCache.RollbackTransaction();
            genDB.RollbackTransaction();
        }
        /// <summary>
        /// Number of objects watched by the cache.
        /// </summary>
        public long CommittedObjectsSize
        {
            get { return iboCache.CommittedObjectsSize; }
        }

        /// <summary>
        /// Updates the statistics of the
        /// generic database. DataContext
        /// must be initialized, and database 
        /// must have been created.
        /// </summary>
        public void UpdateDBStatistics()
        {
            genDB.UpdateStaticstics();
        }

        /// <summary>
        /// Number of uncomitted objects in cache.
        /// </summary>
        public long UnCommittedObjectsSize
        {
            get { return iboCache.UnCommittedObjectsSize; }
        }
    }
}
