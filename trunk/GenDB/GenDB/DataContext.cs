using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB
{
    public class DataContext
    {
        static DataContext instance = new DataContext();

        public Table<T> CreateTable<T> ()
            where T : IBusinessObject
        {
            return new Table<T>(GenDB, Translators, TypeSystem, IBOCache);
        }

        BOListFactory bolistFactory;

        public BOListFactory BolistFactory
        {
            get { return bolistFactory; }
            set { bolistFactory = value; }
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
            Init();
        }

        private void Init()
        {
            // Instantiation order is vital!
            translators = new TranslatorSet(this);
            genDB = new MsSql2005DB (this);
            if (RebuildDatabase)
            {
                if (genDB.DatabaseExists())
                {
                    genDB.DeleteDatabase();
                }
            }
            if (!genDB.DatabaseExists())
            {
                genDB.CreateDatabase();
            }
            typeSystem = new TypeSystem (this);
            typeSystem.Init();
            iboCache = new IBOCache(this);
            //TODO: Kan dette ikke udelades?
            //if (!TypeSystem.IsTypeKnown (typeof(AbstractBusinessObject)))
            //{
            //    TypeSystem.RegisterType(typeof(AbstractBusinessObject));
            //}
            bolistFactory = new BOListFactory(TypeSystem);
        }

        private int dbBatchSize = 1;

        public int DbBatchSize
        {
            get { return dbBatchSize; }
            set { dbBatchSize = value; }
        } 

        private bool rebuildDatabase = true;

        public bool RebuildDatabase
        {
            get { return rebuildDatabase; }
            set { 
                if (genDB != null && value != rebuildDatabase) { throw new Exception("Request for DB rebuild must be set before the DB is accessed for the first time."); }
                rebuildDatabase = value;
            }
        }

        string connectStringWithDBName = "server=(local);database=generic;Integrated Security=SSPI;connection timeout=240";

        internal string ConnectStringWithDBName
        {
            get { return connectStringWithDBName; }
            set { connectStringWithDBName = value; }
        }

        string connectStringWithoutDBName = "server=(local);Integrated Security=SSPI;connection timeout=240";

        internal string ConnectStringWithoutDBName
        {
            get { return connectStringWithoutDBName; }
            set { connectStringWithoutDBName = value; }
        }

        string dbname = "generic";

        public string DatabaseName 
        {
            get { return dbname; }
            set { dbname = value; }
        }

        public void SubmitChanges()
        {
            iboCache.FlushToDB();
        }
    }
}
