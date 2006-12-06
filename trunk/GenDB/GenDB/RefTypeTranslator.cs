using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    internal sealed class RefTypeTranslator : IFieldConverter
    {
        private static RefTypeTranslator instance = new RefTypeTranslator();

        public static RefTypeTranslator Instance 
        {
            get { return instance; }
        }

        public string ToPropertyValueString(object o)
        {
            if (o == null)
            {
                return null;
            }
            IBusinessObject ibo = (IBusinessObject)o;
            if (ibo.DBTag != null)
            {
                return ibo.DBTag.EntityPOID.ToString();
            }
            Translator t = Translator.GetTranslator(o.GetType());
            return t.GetEntityPOID(o);
        }

        public object ToObjectRepresentation(string s)
        {
            if (s == null)
            {
                return null;
            }
            long id = long.Parse(s);
            return Translator.GetFromDatabase(id);
        }
    }
}
