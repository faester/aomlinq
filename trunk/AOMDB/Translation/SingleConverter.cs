using System;
using System.Collections.Generic;
using System.Text;

namespace Translation
{
    class SingleConverter : ToStringConverter, IFieldConverter
    {
        public object ToPropertyValue(string propertyvalue) {
            return Convert.ToSingle(propertyvalue);
        }
    }
}
