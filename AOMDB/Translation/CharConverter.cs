using System;
using System.Collections.Generic;
using System.Text;

namespace Translation
{
    class CharConverter : ToStringConverter, IFieldConverter
    {
        public object ToPropertyValue(string propertyvalue)
        {
            return Convert.ToChar(propertyvalue);
        }

    }
}
