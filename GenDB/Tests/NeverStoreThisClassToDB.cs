using System;
using System.Collections.Generic;
using System.Text;
using GenDB;

namespace Tests
{
    /// <summary>
    /// This class is used to test for correct database 
    /// behaviour if Table&lt;NeverStoreThisClassToDB&gt;.Clear()
    /// is called, and the type T of Table is unknown. 
    /// So the name should be taken literally: It must not be 
    /// persisted, for the tests to run correctly.
    /// </summary>
    class NeverStoreThisClassToDB : AbstractBusinessObject
    {
    }
}
