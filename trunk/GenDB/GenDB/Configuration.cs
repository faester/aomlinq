using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB
{
    public static class Configuration
    {
        static Configuration()
        {
            if (!TypeSystem.IsTypeKnown (typeof(AbstractBusinessObject)))
            {
                TypeSystem.RegisterType(typeof(AbstractBusinessObject));
            }
        }

        private static int dbBatchSize = 1;

        public static int DbBatchSize
        {
            get { return Configuration.dbBatchSize; }
            set { Configuration.dbBatchSize = value; }
        } 

        static IGenericDatabase genDB = null;

        static internal IGenericDatabase GenDB
        {
            get {
                if (genDB == null)
                {
                    genDB = MsSql2005DB.Instance;
                    if (Configuration.RebuildDatabase)
                    {
                        Console.WriteLine("Rebuilding db.");
                        if (genDB.DatabaseExists()){
                            genDB.DeleteDatabase();
                        }
                        genDB.CreateDatabase();
                    }
                    else
                    {
                        Console.WriteLine("NOT rebuilding db.");
                    }
                }
                return genDB; 
            }
        }

        private static bool rebuildDatabase = true;

        public static bool RebuildDatabase
        {
            get { return rebuildDatabase; }
            set { 
                if (genDB != null && value != rebuildDatabase) { throw new Exception("Request for DB rebuild must be set before the DB is accessed for the first time."); }
                rebuildDatabase = value;
            }
        }

        static string connectStringWithDBName = "server=(local);database=generic;Integrated Security=SSPI;connection timeout=240";

        public static string ConnectStringWithDBName
        {
            get { return Configuration.connectStringWithDBName; }
            set { Configuration.connectStringWithDBName = value; }
        }

        static string connectStringWithoutDBName = "server=(local);Integrated Security=SSPI;connection timeout=240";

        public static string ConnectStringWithoutDBName
        {
            get { return Configuration.connectStringWithoutDBName; }
            set { Configuration.connectStringWithoutDBName = value; }
        }

        static string dbname = "generic";

        static public string DatabaseName 
        {
            get { return dbname; }
            set { dbname = value; }
        }

        public static void SubmitChanges()
        {
            IBOCache.FlushToDB();
        }
    }
}
