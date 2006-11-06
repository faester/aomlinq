using System;
using System.Collections.Generic;
using System.Text;
using Persistence;

namespace Business
{
    public interface IBusinessObject
    {
        /// <summary>
        /// Used by the database to determine if an object 
        /// has already been saved. If DatabaseID is null 
        /// when the IBusinessObject is sent to the database,
        /// a new Entity will be created, and the DatabaseID 
        /// i accordance with this.
        /// <p>
        /// Will also provide call-back to the database, when 
        /// an IBusinessObject is garbage collected. The DBTag 
        /// object should NOT be modified by the user.
        /// </p>
        /// </summary>
        DBTag DatabaseID { get; set; }

        /// <summary>
        /// Indicates whether the objects has changed 
        /// its state since last DB-rewrite.
        /// <p>
        /// The object will be stored if DatabaseID is null 
        /// or IsDirty is true.
        /// </p>
        /// </summary>
        bool IsDirty { get; set; }
    }
}
