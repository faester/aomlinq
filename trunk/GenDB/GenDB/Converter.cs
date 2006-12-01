using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace GenDB
{
    /// <summary>
    /// The Converter class wraps an IFieldConverter, a FieldInfo and 
    /// a Property in a triple, that describes a field of an object 
    /// and its relation to the database. FieldInfo describes which field to
    /// extract. Property describes which correspondence with DB Property
    /// and the IFIeldConverter describes how to translate between 
    /// GenericDB and regular object value representation.
    /// <para>
    /// The Converter class is not type safe but assumes that input 
    /// objects are of type ConversionType. 
    /// </para>
    /// </summary>
    internal class Converter
    {
        IFieldConverter conv;
        FieldInfo fi;
        Property p;

        internal FieldInfo FieldInfo
        {
            get { return fi; }
        }

        internal Property Property
        {
            get { return p; }
        }

        internal Type ConversionType
        {
            get { return fi.ReflectedType; }
        }

        private Converter () { /* empty */ }

        public Converter (IFieldConverter conv, FieldInfo fi, Property p)
        {
#if DEBUG
            Console.Out.WriteLine("Converter for field {0} in type {1} instantiated.", fi, fi.ReflectedType);
#endif
            this.conv = conv;
            this.fi = fi;
            this.p = p;
        }

        /// <summary>
        /// Extracts the value of the field represented by this
        /// Converter from object o and converts it to a string
        /// value suitable to store in the DB.
        /// PRE: o must be an instance of type ConversionType
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public string ToPropertyValueString(object o)
        {
            object fieldValue = fi.GetValue (o);
            return conv.ToPropertyValueString(fieldValue);
        }

        /// <summary>
        /// Converts from string repræsentation of
        /// the internal database to regular object 
        /// repræsentation. 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public object ToObjectRepresentation(string s)
        {
            return conv.ToObjectRepresentation(s);
        }

        /// <summary>
        /// Sets the value of field represented by 
        /// this converter in object o, based on value 
        /// PRE: o must be an instance of type ConversionType
        /// string s.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="s"></param>
        public void SetObjectsFieldValue(object o, string s)
        {
            fi.SetValue(o, ToObjectRepresentation(s));
        }
    }
}
