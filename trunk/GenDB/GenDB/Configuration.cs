using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    public static class Configuration
    {
        private static int dbBatchSize = 100;

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
                        if (genDB.DatabaseExists()){
                            genDB.DeleteDatabase();
                        }
                        genDB.CreateDatabase();
                    }
                }
                return genDB; 
            }
        }

        private static bool rebuildDatabase = false;

        public static bool RebuildDatabase
        {
            get { return rebuildDatabase; }
            set { 
                if (genDB != null) { throw new Exception("Request for DB rebuild must be set before the DB is accessed for the first time."); }
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
    }
}
