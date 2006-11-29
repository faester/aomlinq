using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Query;

namespace GenDB
{
    internal class Translator
    {
        static IBOCache cache = IBOCache.Instance;
        static Dictionary<Type, Translator> translators = new Dictionary<Type, Translator>();

        public static Translator GetTranslator(Type t)
        {
            Translator res;
            if (translators.TryGetValue(t, out res))
            {
                return res;
            }
            res = new Translator(t);
            translators[t] = res;
            return res;
        }


        FieldInfo[] fields;        
        Type objectType;
        LinkedList<Property> Properties;
        EntityType et;

        private Translator superTranslator = null;

        private Translator() { /* empty */ }

        private Translator(Type objectType)
        {
            this.objectType = objectType;
            Init();
        }

        private void InitEntityType()
        {
            et = GenericDB.Instance.GetEntityType (objectType.FullName);
            if (objectType.BaseType != null)
            {
                superTranslator = GetTranslator(objectType.BaseType);
            }
        }

        private void InitProperties()
        {
            // Der skal testes for om feltet er en referencetype her.
            // Felttyper (primitiver) hentes fra FieldConverters
            
        }

        private void Init()
        {
            InitEntityType();
            InitProperties();
        }
    }
}
