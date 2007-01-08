using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Query;
using System.Expressions;
using GenDB.DB;

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

    internal interface IDBSaveableCollection
    {
        void SaveElementsToDB();
    }
  

    ///// <summary>
    ///// Gem typerne for K og V som properties på objektet og giv disse faste 
    ///// navne i TypeSystem
    ///// TODO: BODictionary
    ///// </summary>
    ///// <typeparam name="K"></typeparam>
    ///// <typeparam name="V"></typeparam>
    //public class BODictionary<K, V> : AbstractBusinessObject, IDictionary<K, V>, IDBSaveableCollection
    //{
    //    DBTag dbtag;

    //    public DBTag DBTag
    //    {
    //        get { return dbtag; }
    //        set { dbtag = value; }
    //    }
    //}
}
