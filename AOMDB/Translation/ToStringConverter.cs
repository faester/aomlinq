using System;
using System.Collections.Generic;
using System.Text;

namespace Translation
{
    /// <summary>
    /// Most objects are converted using their ToString
    /// methods. This abstract class defines such a method
    /// and is ment to be super type of most field converters.
    /// </summary>
    abstract class ToStringConverter {
        public string ToValueString(object o){
            return o.ToString();
        }

        public bool CanHandleEquals
        {
            get { return true; }
        }
    }
}
