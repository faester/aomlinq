using System;
using System.Collections.Generic;
using System.Text;

namespace Translation
{
    class Int32Converter : ToStringConverter, IFieldConverter
    {
        public object ToPropertyValue(string propertyvalue)
        {
            return Convert.ToInt32(propertyvalue);
        }
    }
}
