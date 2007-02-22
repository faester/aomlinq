using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB
{
    public class DataContext
    {
        static DataContext instance = new DataContext();

        bool isInitialized = false;

        public bool IsInitialized
        {
            get { return isInitialized; }
        }

        public Table<T> GetTable<T> ()
            where T : IBusinessObject
        {
            if (!isInitialized)
            {
                Init();
            }
            return new Table<T>(GenDB, Translators, TypeSystem, IBOCache);
        }

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
            // Instantiation order is vital!
            translators = new TranslatorSet(this);
            genDB = new MsSql2005DB (this);
        }

        public void Init()
        {
            if (isInitialized)
            {
                throw new Exception("Database already up running.");
            }
            iboCache = new IBOCache(this);

            typeSystem = new TypeSystem (this);
            typeSystem.Init();
             
            
            isInitialized = true;
        }

        private int dbBatchSize = 1;

        /// <summary>
        /// Attempts to build database. All changes will be lost. 
        /// Cache will be emptied without committing. Should only 
        /// be used as part of program initialization before any-
        /// thing has been added to the database.
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
        /// of program initialization.
        /// </summary>
        public void CreateDatabase()
        {
            if (isInitialized)
            {
                throw new Exception("Database already up running.");
            }
            genDB.CreateDatabase();
        }

        public bool DatabaseExists()
        {
            return genDB.DatabaseExists();
        }

        public int DbBatchSize
        {
            get { return dbBatchSize; }
            set { dbBatchSize = value; }
        }

        private int commandTimeout = 0;

        public int CommandTimeout
        {
            get { return commandTimeout; }
            set { commandTimeout = value; }
        }


        string connectStringWithDBName = "server=(local);database=generic;Integrated Security=SSPI;connection timeout=0";

        internal string ConnectStringWithDBName
        {
            get { return connectStringWithDBName; }
            set { connectStringWithDBName = value; }
        }

        string connectStringWithoutDBName = "server=(local);Integrated Security=SSPI;connection timeout=0";

        internal string ConnectStringWithoutDBName
        {
            get { return connectStringWithoutDBName; }
            set { connectStringWithoutDBName = value; }
        }

        string dbname = "generic";

        public string DatabaseName 
        {
            get { return dbname; }
            set { 
                dbname = value; 
                connectStringWithDBName = connectStringWithoutDBName + ";database=" + value;
            }
        }

        public void SubmitChanges()
        {
            iboCache.SubmitChanges();
        }

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
        /// Number of uncomitted objects in cache.
        /// </summary>
        public long UnCommittedObjectsSize
        {
            get { return iboCache.UnCommittedObjectsSize; }
        }
    }
}
