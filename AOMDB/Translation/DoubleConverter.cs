using System;

namespace Translation
{
    class DoubleConverter : ToStringConverter, IFieldConverter
    {
        public object ToPropertyValue(string propertyValue)
        {
            return Convert.ToDouble (propertyValue);
        }
    }
}
