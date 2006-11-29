using System;

namespace Translation
{
    class DateTimeConverter : IFieldConverter
    {
        public string ToValueString(object o)
        {
            DateTime dt = (DateTime)o;
            return dt.Ticks.ToString();
        }

        public object ToPropertyValue(string propertyValue)
        {
            long ticks = long.Parse(propertyValue);
            return new DateTime(ticks);
        }

        public  bool CanHandleEquals
        {
            get { return true; }
        }
    }
}
