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
}
