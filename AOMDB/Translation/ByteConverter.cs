using System;
using System.Collections.Generic;
using System.Text;

namespace Translation
{
    class ByteConverter : ToStringConverter, IFieldConverter
    {
        public object ToPropertyValue(string s)
        {
            return Convert.ToByte(s);
        }
    }
}
