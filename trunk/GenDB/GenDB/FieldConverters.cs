using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    internal interface IFieldConverter
    {
        string ToPropertyValueString(object o);
        object ToObjectRepresentation(string s);

    }

    internal class StringConverter : IFieldConverter
    {
        public string ToPropertyValueString(object o)
        {
            return o.ToString();
        }
        public object ToObjectRepresentation(string s)
        {
            return s;
        }
    }

    internal class Int32Converter : IFieldConverter
    {
        public string ToPropertyValueString(object o)
        {
            return o.ToString();
        }
        public object ToObjectRepresentation(string s)
        {
            return Int32.Parse (s);
        }
    }

    static class FieldConverters
    {
        static Dictionary<Type, IFieldConverter> convs = new Dictionary<Type, IFieldConverter>();
        static FieldConverters()
        {
            convs[typeof(string)] = new StringConverter();
            convs[typeof(Int32)] = new StringConverter();
        }

        public static IFieldConverter GetConverter(Type t)
        {
            try {
                return convs[t];
            }
            catch (KeyNotFoundException)
            {
                throw new UnconvertibleFieldException(t.FullName);
            }
        }
    }

    public class UnconvertibleFieldException : Exception 
    {
        public UnconvertibleFieldException(string name) : base(name) { }
    }
}