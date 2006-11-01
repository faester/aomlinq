using System;
using System.Collections.Generic;
using System.Text;

namespace Translation
{
    class LongConverter : ToStringConverter, IFieldConverter
    {
        public object ToPropertyValue(string propertyvalue) {
            return Convert.ToInt64(propertyvalue);
        }
    }
}
