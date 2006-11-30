using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Query;

namespace GenDB
{

    internal class Translator : IFieldConverter
    {
        #region static part
        static Type[] EMPTY_TYPE_ARRAY = new Type[0];
        static IBOCache cache = IBOCache.Instance;
        static Dictionary<Type, Translator> type2translator = new Dictionary<Type, Translator>();
        static Dictionary<long, Translator> et2translator = new Dictionary<long, Translator>();
        static Type DBTAG_TYPE = typeof(DBTag);

        /// <summary>
        /// Used to create new translator instances as
        /// well as retrieving already constructed translators
        /// based on a Type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal static Translator GetCreateTranslator(Type t)
        {
            Translator res;
            if (type2translator.TryGetValue(t, out res))
            {
                return res;
            }
            res = new Translator(t);
            type2translator[t] = res;
            long entityTypePOID = res.et.EntityTypePOID;
            et2translator [entityTypePOID] = res;
            return res;
        }


        /// <summary>
        /// Used to retrive Translators based on an
        /// EntityTypePOID. If a corresponding translator does
        /// not exist, one is constructed based on the
        /// EntityType.Name. 
        /// </summary>
        /// <param name="entityTypePOID"></param>
        /// <returns></returns>
        internal static Translator GetTranslator(long entityTypePOID)
        {
            Translator res;
            if (et2translator.TryGetValue(entityTypePOID, out res))
            {
                return res;
            }

            EntityType et = (from ets in GenericDB.Instance.EntityTypes
                             where ets.EntityTypePOID == entityTypePOID
                             select ets).First();
            Type t = Type.GetType(et.Name);
            res = GetCreateTranslator (t);
            return res;
        }

        /// <summary>
        /// Return s an object identified by the
        /// given id from the database.
        /// PRE: Behaviour is unspecified if no objects
        /// where EntityPOID == id exists in the 
        /// database. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static IBusinessObject GetFromDatabase(long id)
        {
            // Can be a subtype of objectType, so we need to 
            // retrieve a translator for each method invokation.
            Entity e = (from es in GenericDB.Instance.Entities
                       where es.EntityPOID == id
                       select es).First();

            Translator st = Translator.GetTranslator (e.EntityType.EntityTypePOID);
            object o = st.NewObjectInstance();

            foreach (Converter c in st.allConverters)
            {
                PropertyValue pv = e.PropertyValues.Where ( (PropertyValue tpv) => tpv.Entity == e ).First();
                c.FieldInfo.SetValue (o, c.ToObjectRepresentation(pv.TheValue));
            }

            return (IBusinessObject)o;
        }

        /// <summary>
        /// Checks if a given type can be translated 
        /// using the Translator class. If the type 
        /// is accepted, nothing happens, but an error
        /// is thrown in case the Type is not translatable.
        /// </summary>
        /// <param name="t"></param>
        private static void CheckRefTypeLegality(Type t)
        {
            if (t.IsGenericType || t.IsGenericTypeDefinition)
            {
                throw new NotTranslatableException("Can not translate generic types.");
            }

            if (t.IsArray)
            {
                throw new NotTranslatableException("Can not translate arrays.");
            }

            if (t == typeof(IBusinessObject)) { return; }

            Type ibusinessobjectInterface = t.GetInterface(typeof(IBusinessObject).FullName);
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
                superTranslator = GetCreateTranslator(objectType.BaseType);
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
                throw new Exception ("Can not handle objecst with static fields.");
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
                        Translator t = Translator.GetCreateTranslator(fi.FieldType);
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
            if (ibo.DBTag == null)
            {
                // Add business object to database
                // (Assigns DBTag)
                InsertNewDBEntity(ibo);
            }
            return ibo.DBTag.EntityPOID.ToString();
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

        private void UpdateDBWith(IBusinessObject o)
        {
            if (o.DBTag != null) // test if o is already in DB
            {
                UpdateDBEntity(o);
            }
            else
            {
                InsertNewDBEntity (o);
            }
        }

        private void UpdateDBEntity(IBusinessObject o)
        {
            //Select Entity with matching id
            Entity e = (from es in GenericDB.Instance.Entities
                           where es.EntityPOID == o.DBTag.EntityPOID
                           select es).First();

            //Copy all property values to dictionary with PropertyPOID as key
            IDictionary<long, PropertyValue> pvs = new Dictionary<long, PropertyValue>();
            pvs = e.PropertyValues.ToDictionary((PropertyValue pv) => pv.PropertyPOID);

            // Step through Converters properties and assign value to corresponding PropertyValue
            foreach (Converter c in allConverters)
            {
                pvs[c.Property.PropertyPOID].TheValue = c.ToPropertyValueString(o);
            }

            GenericDB.Instance.SubmitChanges();
        }

        /// <summary>
        /// Constructs and Entity in accordance with o
        /// and adds it to the database.
        /// </summary>
        /// <param name="o"></param>
        private void InsertNewDBEntity(IBusinessObject o)
        {
            Entity e = new Entity();
            e.EntityType = et;

            foreach (Converter c in allConverters)
            {
                PropertyValue pv = new PropertyValue();
                pv.Property = c.Property;
                if (c.Property == null) { throw new NullReferenceException("c.Property"); }
                pv.Entity = e;
                e.PropertyValues.Add (pv);
                pv.TheValue = c.ToPropertyValueString(o);
                GenericDB.Instance.PropertyValues.Add(pv);
            }
            GenericDB.Instance.Entities.Add (e);
            GenericDB.Instance.SubmitChanges();
            DBTag.AssignDBTagTo(o, e.EntityPOID, IBOCache.Instance);
        }

        /// <summary>
        /// Creates an instance of the type this
        /// Translator represents.
        /// </summary>
        /// <returns></returns>
        private object NewObjectInstance()
        {
            Type t = Type.GetType(et.Name);
            ConstructorInfo ci = t.GetConstructor(EMPTY_TYPE_ARRAY);
            object o = ci.Invoke(null);
            return o;
        }

    }

    public class NotTranslatableException : Exception
    {
        public NotTranslatableException(string msg)
            : base(msg) { /* empty */ }
    }
}
