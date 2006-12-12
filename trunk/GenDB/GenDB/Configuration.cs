using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    static class Configuration
    {
        static IGenericDatabase genDB = null;

        static internal IGenericDatabase GenDB
        {
            get {
                if (genDB == null) { genDB = MsSql2005DB.Instance; }
                return genDB; 
            }
        }

        static string connectStringWithDBName = "server=(local);database=generic;Integrated Security=SSPI";

        public static string ConnectStringWithDBName
        {
            get { return Configuration.connectStringWithDBName; }
            set { Configuration.connectStringWithDBName = value; }
        }

        static string connectStringWithoutDBName = "server=(local);Integrated Security=SSPI";

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
