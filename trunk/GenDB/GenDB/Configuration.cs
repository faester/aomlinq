using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    static class Configuration
    {
        static IGenericDatabase genDB = MsSql2005DB.Instance;

        static internal IGenericDatabase GenDB
        {
            get { return genDB; }
            set { genDB = value; }
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
