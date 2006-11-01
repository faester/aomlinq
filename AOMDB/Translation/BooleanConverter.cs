using System;
using System.Collections.Generic;
using System.Text;

namespace Translation
{
    class BooleanConverter : ToStringConverter, IFieldConverter
    {
        public object ToPropertyValue(string propertyValue)
        {
            return Convert.ToBoolean (propertyValue);
        }
    }
}
