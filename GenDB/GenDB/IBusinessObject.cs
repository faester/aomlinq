using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;
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
        DBIdentifier DBIdentity { get; set; }
    }

    internal interface IDBSaveableCollection
    {
        void SaveElementsToDB();
        bool HasBeenModified {get; set;}
    }

}
