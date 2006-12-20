using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    public interface IBusinessObject
    {
        /// <summary>
        /// NB: The DBTag is essential to the persistence 
        /// system and should not be referenced or 
        /// modified by the user!
        /// </summary>
        DBTag DBTag { get; set; }
    }

    public class BODictionary<K, V> : Dictionary<K, V>, IBusinessObject
    {
        DBTag dbtag;

        public DBTag DBTag
        {
            get { return dbtag; }
            set { dbtag = value; }
        }

        
    }
}
