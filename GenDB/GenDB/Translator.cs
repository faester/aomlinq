using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Query;

namespace GenDB
{
    internal class Converter
    {
        IFieldConverter conv;
        FieldInfo fi;
        public Converter (IFieldConverter conv, FieldInfo fi)
        {
            this.conv = conv;
            this.fi = fi;
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
    }

    internal class Translator : IFieldConverter 
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

        private static void CheckTypeLegality(Type t)
        {
            if (t.IsGenericType || t.IsGenericTypeDefinition)
            {
                throw new NotTranslatableException("Can not translate generic types.");
            }
            Type ibusinessobjectInterface =
                t.GetInterface(typeof(IBusinessObject).FullName);
            if (ibusinessobjectInterface == null)
            {
                throw new NotTranslatableException("Reference types must implement IBusinessObject (" + t.FullName + ")" );
            }
        }


        LinkedList<Converter> declaredConverters = new LinkedList<Converter>(); // Contains converters for declared fields
        LinkedList<Converter> allConverters = new LinkedList <Converter>(); // All converters needed to convert between objectType and generic representation

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
            fields = objectType.GetFields(BindingFlags.NonPublic
                                         |BindingFlags.Public
                                         |BindingFlags.DeclaredOnly
                                         );

            if (objectType.GetFields( BindingFlags.Static ).Length != 0)
            {
                throw new Exception ("Can not handle static fields.");
            }
                     
            foreach (FieldInfo fi in fields)
            {
                if (fi.FieldType.IsValueType || fi.FieldType.Equals(typeof(string)))
                {
                    declaredConverters.AddLast (new Converter(FieldConverters.GetConverter(fi.FieldType), fi));
                }
                else 
                {
                    CheckTypeLegality(fi.FieldType); // Check if we can translate. (Throws exception on error)
                }
            }
            // Der skal testes for om feltet er en referencetype her.
            // Felttyper (primitiver) hentes fra FieldConverters
        }

        private void Init()
        {
            InitEntityType();
            InitProperties();
        }

        public string ToPropertyValueString(object o)
        {
            if (o == null) return null;
            // Translators should never accept anything but 
            // primitive types, string and IBusinassObject 
            // instances, so the cast below should be safe.
            IBusinessObject ibo = (IBusinessObject)o;
            throw new Exception("Not implemented.");
        }

        public object ToObjectRepresentation(string s)
        {
            if (s == null) return null;

            long id = long.Parse (s);

            IBusinessObject res = IBOCache.Instance.Get (id);

            throw new Exception("Not implemented.");
        }
    }

    public class NotTranslatableException : Exception
    {
        public NotTranslatableException(string msg)
            : base(msg) { /* empty */ }
    }
}
