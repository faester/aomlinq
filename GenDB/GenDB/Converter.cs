using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace GenDB
{
    internal class Converter
    {
        IFieldConverter conv;
        FieldInfo fi;
        Property p;

        internal FieldInfo FieldInfo
        {
            get { return fi; }
            set { fi = value; }
        }

        internal Property Property
        {
            get { return p; }
            set { p = value; }
        }

        public Converter (IFieldConverter conv, FieldInfo fi, Property p)
        {
            this.conv = conv;
            this.fi = fi;
            this.p = p;
        }

        public string ToPropertyValueString(object o)
        {
            object fieldValue = fi.GetValue (o);
            return conv.ToPropertyValueString(fieldValue);
        }

        public object ToObjectRepresentation(string s)
        {
            return conv.ToObjectRepresentation(s);
        }

        public void SetObjectsFieldValue(object o, string s)
        {
            fi.SetValue(o, ToObjectRepresentation(s));
        }
    }
}
