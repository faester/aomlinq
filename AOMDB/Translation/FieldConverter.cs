using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using AOM;
using Persistence;

namespace Translation
{

    class DefaultFieldConverter : ToStringConverter, IFieldConverter
    {
        public object ToPropertyValue(string pv) {
            return pv;
        }
    }

    /// <summary>
    /// This static class is used to convert Property value 
    /// strings to suitable objects. 
    /// </summary>
    public static class FieldConverter 
    {
        static IFieldConverter defaultConverter = new DefaultFieldConverter();
        static Dictionary<Type, IFieldConverter> converters = new Dictionary<Type, IFieldConverter>();

        /// <summary>
        /// Initializes the default set of converters.
        /// </summary>
        static FieldConverter ()
        {
            converters.Add (typeof(Int32), new Int32Converter());
            converters.Add (typeof(Int64), new LongConverter());
            converters.Add (typeof(Char), new CharConverter());
            converters.Add (typeof(Single), new SingleConverter());
            converters.Add (typeof(Double), new DoubleConverter());
            converters.Add (typeof(Boolean), new BooleanConverter());
            converters.Add (typeof(DateTime), new DateTimeConverter());
            converters.Add (typeof(Byte), new ByteConverter());
        }

        /// <summary>
        /// Converters for enum s_types. Will
        /// create appropriate converter if 
        /// none is instantiated upon calling.
        /// This converter will be stored in the 
        /// converters set and returned upon next        /// instantiation.
        /// </summary>
        /// <param name="t">Must be enum type</param>
        /// <returns>Converter for an enum type.</returns>
        private static IFieldConverter GetEnumConverter(Type t) {
            IFieldConverter res = null;
            if (converters.TryGetValue(t, out res)) {
                return res;
            } else {
                res = new EnumConverter(t);
                converters.Add (t, res);
                return res;
            }
        }

        private static IFieldConverter GetReferenceConverter(Type t) 
        {
            throw new Exception("Unimplemented method");
        }

        private static IFieldConverter GetConverter(Type t) {
            if (t.IsEnum) {
                return GetEnumConverter(t);
            }
            IFieldConverter res = null;
            if (converters.TryGetValue(t, out res)) {
                return res;
            } else {
                return defaultConverter;
            }
        }

        internal static object ToObject(string entityPOID) 
        {
            object res = RefTypeConverter.Instance.ToPropertyValue(entityPOID);
            return res;
        }

        internal static string ToEntityPOIDString(object o)
        {
            string res = RefTypeConverter.Instance.ToValueString(o);
            return res;
        }

        public static object ToFieldValue(FieldInfo f, string value)
        {
            return GetConverter(f.FieldType).ToPropertyValue(value);
        }

        public static string ToValueString(FieldInfo f, object value) 
        {
            return GetConverter(f.FieldType).ToValueString(value);
        }
    }
}
