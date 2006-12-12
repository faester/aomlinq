//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace GenDB
//{
//    internal interface IFieldConverter
//    {
//        string ToPropertyValueString(object o);
//        object ToObjectRepresentation(string s);

//    }

//    internal class StringConverter : IFieldConverter
//    {
//        public string ToPropertyValueString(object o)
//        {
//            if (o == null) return null;
//            return o.ToString();
//        }
//        public object ToObjectRepresentation(string s)
//        {
//            if (s == null) return null;
//            return s;
//        }
//    }

//    internal class DateTimeConverter : IFieldConverter
//    {
//        public string ToPropertyValueString(object o)
//        {
//            if (o == null) return null;
//            long tick = ((DateTime)o).Ticks;
//            return tick.ToString();
//        }
//        public object ToObjectRepresentation(string s)
//        {
//            if (s == null) return null;
//            long ticks = long.Parse(s);
//            DateTime res = new DateTime(ticks);
//            return res;
//        }
//    }

//    internal class Int32Converter : IFieldConverter
//    {
//        public string ToPropertyValueString(object o)
//        {
//            return o.ToString();
//        }
//        public object ToObjectRepresentation(string s)
//        {
//            return Int32.Parse (s);
//        }
//    }

//    static class FieldConverters
//    {
//        static Dictionary<Type, IFieldConverter> convs = new Dictionary<Type, IFieldConverter>();
//        static FieldConverters()
//        {
//            convs[typeof(string)] = new StringConverter();
//            convs[typeof(Int32)] = new Int32Converter();
//            convs[typeof(DateTime)] = new DateTimeConverter();
//        }

//        public static IFieldConverter GetConverter(Type t)
//        {
//            IFieldConverter res = null;
//            try {
//                res =  convs[t];
//                return res;
//            }
//            catch (KeyNotFoundException)
//            {
//                throw new UnconvertibleFieldException(t.FullName);
//            }
//        }
//    }

//    public class UnconvertibleFieldException : Exception 
//    {
//        public UnconvertibleFieldException(string name) : base(name) { }
//    }
//}