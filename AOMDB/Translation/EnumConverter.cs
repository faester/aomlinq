using System;

namespace Translation
{
    class EnumConverter : ToStringConverter, IFieldConverter
    {
        Type enumType = null;

        private EnumConverter() { /* empty */ }

        public EnumConverter(Type enumType) {
            this.enumType = enumType;
        }

        public object ToPropertyValue(string propertyValue) 
        {
            return Enum.Parse(enumType, propertyValue);
        }
    }
}
