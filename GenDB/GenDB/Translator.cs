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
        Property p;

        internal Property Property
        {
            get { return p; }
            set { p = value; }
        }

        public Converter (IFieldConverter conv, FieldInfo fi, Property p)
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
        #region static part
        static IBOCache cache = IBOCache.Instance;
        static Dictionary<Type, Translator> translators = new Dictionary<Type, Translator>();
        static Type DBTAG_TYPE = typeof(DBTag);

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

        private static void CheckRefTypeLegality(Type t)
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

        private static bool IsValueTranslatable (Type t)
        {
            return t.IsValueType || t.Equals(typeof(string));
        }
        #endregion

        LinkedList<Converter> declaredConverters = new LinkedList<Converter>(); // Contains converters for declared fields
        IEnumerable<Converter> allConverters = new LinkedList <Converter>(); // All converters needed to convert between objectType and generic representation

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
            et = GenericDB.Instance.GetCreateEntityType (objectType.FullName);
            if (objectType.BaseType != null)
            {
                superTranslator = GetTranslator(objectType.BaseType);
            }
        }

        private void InitProperties()
        {
            fields = objectType.GetFields(BindingFlags.NonPublic
                                         | BindingFlags.Instance 
                                         | BindingFlags.Public
                                         | BindingFlags.DeclaredOnly
                                         );

            if (objectType.GetFields( BindingFlags.Static ).Length != 0)
            {
                throw new Exception ("Can not handle static fields.");
            }
                     
            foreach (FieldInfo fi in fields)
            {
                if (fi.FieldType != DBTAG_TYPE) // DBTags should never be stored in database.
                {
                    var pc = from p in GenericDB.Instance.Properties
                             where p.Name == fi.Name && p.EntityType == et
                             select p;

                    Property property;

                    if (pc.Count() == 0)
                    { // Property did not exist, so create it and add it to DB.
                        property = new Property();
                        property.EntityType = et;
                        property.Name = fi.Name;
                        PropertyType pt = GenericDB.Instance.GetCreatePropertyType(fi.FieldType.FullName);
                        property.PropertyType = pt;
                        et.Properties.Add(property);
                    }
                    else
                    {
                        property = pc.First();
                    }
                    if (Translator.IsValueTranslatable(fi.FieldType))
                    {
                        declaredConverters.AddLast(new Converter(FieldConverters.GetConverter(fi.FieldType), fi, property));
                    }
                    else
                    {
                        Translator.CheckRefTypeLegality(fi.FieldType); // Check if we can translate. (Throws exception on error)
                        Translator t = Translator.GetTranslator(fi.FieldType);
                        declaredConverters.AddLast(new Converter(t, fi, property));
                    }
                }
            }

            if (superTranslator != null)
            {
                allConverters = declaredConverters.Union(superTranslator.GetConverters());
            }
            else 
            {
                allConverters = declaredConverters;
            }
        }

        private IEnumerable<Converter> GetConverters()
        {
            return allConverters;
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
            // primitive types, string (Handled by the 
            // IFieldConverter classes) whereas this Trans-
            // should only handle IBusinassObject 
            // instances, so the cast below should be safe.
            IBusinessObject ibo = (IBusinessObject)o;
            throw new Exception("Not implemented.");
        }

        public object ToObjectRepresentation(string s)
        {
            if (s == null) return null;

            long id = long.Parse (s);

            IBusinessObject res = IBOCache.Instance.Get(id);

            if (res == null)
            {
                res = GetFromDatabase (id);
                DBTag.AssignDBTagTo (res, id, IBOCache.Instance);
            }

            return res;
        }

        private void AddToDatabase(IBusinessObject o)
        {
            Entity e = new Entity();
            throw new Exception ("Not implemented.");

            
        }

        private IBusinessObject GetFromDatabase(long id)
        {
            throw new Exception ("Not implemented.");
        }
    }

    public class NotTranslatableException : Exception
    {
        public NotTranslatableException(string msg)
            : base(msg) { /* empty */ }
    }
}
