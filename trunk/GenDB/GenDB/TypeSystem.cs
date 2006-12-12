using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace GenDB
{
    /// <summary>
    /// Vedligeholder typebeskrivelser og overs�ttere til de forskellige klasser.
    /// 
    /// Aktuelt skaber det Exceptions, hvis der �ndres i typer i en eksisterende DB.
    /// Det skal formentlig udbedres, da det i nogen grad fjerner ideen i en generisk
    /// database. Der skal dog fastl�gges en semantik omkring dette, og det er ogs� 
    /// muligt vi skal v�lge at acceptere, at databasen skal nulstilles, n�r man �ndrer
    /// i klassehierarkiet. Det er trods alt ikke afg�rende for at unders�ge, om man 
    /// kan f� skidtet til at performe effektivt.
    /// </summary>
    class TypeSystem
    {
        #region Singleton stuff
        static TypeSystem instance = new TypeSystem();

        internal static TypeSystem Instance
        {
            get { return instance; }
        }
        #endregion

        private Dictionary<long, IETCacheElement> etid2IEt = new Dictionary<long, IETCacheElement> ();
        private Dictionary<string, IETCacheElement> name2IEt = new Dictionary<string, IETCacheElement> ();
        private Dictionary<Type, IETCacheElement> type2IEt = new Dictionary<Type, IETCacheElement> ();
        
        private Dictionary<long, IPropertyType> ptid2pt = new Dictionary<long, IPropertyType>();
        private Dictionary<string, IPropertyType> ptName2pt = new Dictionary<string, IPropertyType>();

        private TypeSystem()
        {
            Init();
        }

        /// <summary>
        /// After calling this method, all EntityTypes 
        /// from database must be cached in memory. 
        /// 
        /// New EntityTypes must be created through
        /// this typesystem instance which will take care
        /// og persisting entity types, property types and
        /// properties.
        /// </summary>
        private void Init()
        {
            foreach (IEntityType ets in Configuration.GenDB.GetAllEntityTypes())
            {
                RegisterType(ets);
            }
            foreach (IPropertyType pt in Configuration.GenDB.GetAllPropertyTypes())
            {
                ptid2pt.Add (pt.PropertyTypePOID, pt);
                ptName2pt.Add (pt.Name, pt);
            }
        }

        /// <summary>
        /// Registers inheritance structure of the given type
        /// and adds it to the database.
        /// </summary>
        /// <param name="et"></param>
        private void RegisterType(IEntityType et)
        {
            if (etid2IEt .ContainsKey(et.EntityTypePOID))
            {
                return;
            }
            if (et.SuperEntityType != null && !etid2IEt.ContainsKey(et.SuperEntityType.EntityTypePOID))
            {
                RegisterType(et.SuperEntityType);
            }
            Type t = Type.GetType(et.Name);
            if (t == null) { throw new Exception("Could not find a CLR type with name: " + et.Name); }
            IETCacheElement ce = new IETCacheElement(et, t, this);
            // Use add to ensure exception, if something is attempted to be inputted twice.
            etid2IEt.Add (et.EntityTypePOID, ce);
            name2IEt.Add(et.Name, ce);
            type2IEt.Add (ce.ClrType, ce);

            // Register et at its supertype, if one is present
            if (et.SuperEntityType != null)
            {
                etid2IEt[et.SuperEntityType.EntityTypePOID].AddSubType (et);
            }
            Configuration.GenDB.Save (et);
            Configuration.GenDB.CommitTypeChanges();
        }
        internal void RegisterType(Type t)
        {
            if (!type2IEt.ContainsKey(t))
            {
                IEntityType et = ConstructEntityType(t);
                RegisterType(et);
                Configuration.GenDB.Save(et);
            }
        }

        /// <summary>
        /// Returns IEntityType with given entityTypePOID.
        /// Since all IEntityTypes are loaded upon instantiation,
        /// this method does not perform any checks to test if
        /// the ID is legal or not.
        /// </summary>
        /// <param name="entityTypePOID"></param>
        /// <returns></returns>
        public IEntityType GetEntityType(long entityTypePOID)
        {
            return etid2IEt[entityTypePOID].Target;
        }

        public DelegateTranslator GetTranslator (Type t)
        {
            return type2IEt[t].Translator;
        }

        public DelegateTranslator GetTranslator (long entityTypePOID )
        {
            return etid2IEt[entityTypePOID].Translator;
        }

        /// <summary>
        /// Returns the IEntityType given and all sub entity types.
        /// </summary>
        /// <param name="iet"></param>
        /// <returns></returns>
        public IEnumerable<IEntityType> GetTypesInstanceOf(IEntityType iet)
        {
            // List for storing the result
            LinkedList<IEntityType> result = new LinkedList<IEntityType>();

            // Add the element it self to the list
            result.AddLast (iet);

            //Traverse through all direct sub types
            foreach (IEntityType et in etid2IEt[iet.EntityTypePOID].DirectSubTypes)
            {
                // Add the sub type and all its sub types recursively.
                foreach (IEntityType et2 in GetTypesInstanceOf(et))
                {
                    result.AddLast (et2);
                }
            }
            return result;
        }

        public IEnumerable<IEntityType> GetTypesInstanceOf(long entityTypePOID)
        {
            return GetTypesInstanceOf(etid2IEt[entityTypePOID].Target);
        }

        public IEntityType ConstructEntityType(Type t)
        {
            IEntityType et = Configuration.GenDB.NewEntityType(t.FullName);

            et.Name = t.FullName;

            FieldInfo[] fields = t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                        BindingFlags.Public | BindingFlags.NonPublic );

            foreach (FieldInfo field in fields)
            {
                Attribute attr = Attribute.GetCustomAttribute(field, typeof(Volatile));
                if (field.FieldType != typeof(DBTag) && attr == null)
                {
                    IProperty property = Configuration.GenDB.NewProperty();
                    property.PropertyName = field.Name;
                    property.PropertyType = GetPropertyType(field.FieldType.FullName);
                    property.MappingType = FindMappingType(field);
                    property.EntityType = et;
                    et.AddProperty(property);
                }
            }

            Type superType = t.BaseType;
            if (superType != null && superType != typeof(object))
            {
                if (type2IEt.ContainsKey (superType))
                {
                    et.SuperEntityType = type2IEt[superType].Target;
                }
                else
                {
                    IEntityType set = ConstructEntityType (superType);
                    RegisterType(set);
                    et.SuperEntityType = set;
                }
            }
            return et;
        }

        MappingType FindMappingType (FieldInfo field)
        {
            Type t = field.FieldType;
            if (t.IsPrimitive)
            {
                if (t == typeof(int) || t == typeof(long) || t == typeof(short))
                {
                    return MappingType.LONG;
                }
                else if (t == typeof(bool))
                {
                    return MappingType.BOOL;
                }
                else if (t == typeof(char))
                {
                    return MappingType.CHAR;
                }
                else if (t == typeof(float) || t == typeof(double))
                {
                    return MappingType.DOUBLE;
                }
                else 
                {
                    throw new NotTranslatableException("Did not know how to map primitive field", field);
                }
            }
            else if (t.IsArray )
            {
                throw new NotTranslatableException("Can not translate ararys.", field);
            }
            else if (t == typeof(string))
            {
                return MappingType.STRING;
            }
            else if (t == typeof(DateTime))
            {
                return MappingType.DATETIME;
            }
            else if (t.IsByRef || t.IsClass)
            {
                return MappingType.REFERENCE;
            }
            else
            {
                throw new NotTranslatableException("Can not find mappingtype of field.", field);
            }
        }

        public IPropertyType GetPropertyType(string name)
        {
            IPropertyType res;
            if (ptName2pt.TryGetValue(name, out res))
            {
                return res;
            }
            else 
            {
                res = Configuration.GenDB.NewPropertyType();
                res.Name = name;
                ptName2pt.Add (res.Name, res);
                ptid2pt.Add (res.PropertyTypePOID, res);
            }
            return res;
        }

        #region IETCacheElemetn definition
        private class IETCacheElement
        {
            IEntityType entityType;
            Type clrType;
            DelegateTranslator translator;

            public DelegateTranslator Translator
            {
                get { return translator; }
            }

            public Type ClrType
            {
                get { return clrType; }
            }

            public IEntityType Target
            {
                get { return entityType; }
                set { entityType = value; }
            }

            ICollection<IEntityType> directSubTypes = new LinkedList<IEntityType>();

            public IEnumerable<IEntityType> DirectSubTypes
            {
                get { return directSubTypes; }
            }

            public void AddSubType(IEntityType iet)
            {
                directSubTypes.Add(iet);
            }
            
            public IETCacheElement (IEntityType iet, Type t, TypeSystem owner)
            {
                this.clrType = t;
                entityType = iet;
                translator = new DelegateTranslator (t, entityType, owner);
            }
        }
        #endregion
    }
}
