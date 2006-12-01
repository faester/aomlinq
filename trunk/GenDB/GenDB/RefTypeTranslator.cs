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
            IBusinessObject ibo = (IBusinessObject)o;
            if (ibo.DBTag != null)
            {
                return ibo.DBTag.EntityPOID.ToString();
            }
            Translator t = Translator.GetCreateTranslator(o.GetType());
            return t.ToPropertyValueString(o);
        }

        public object ToObjectRepresentation(string s)
        {
            long id = long.Parse(s);
            return Translator.GetFromDatabase(id);
        }
    }
}
