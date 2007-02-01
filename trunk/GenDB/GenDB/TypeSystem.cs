using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GenDB.DB;

namespace GenDB
{
    /// <summary>
    /// Vedligeholder typebeskrivelser og oversættere til de forskellige klasser.
    /// 
    /// Aktuelt skaber det Exceptions, hvis der ændres index typer index en eksisterende DB.
    /// Det skal formentlig udbedres, da det index nogen grad fjerner ideen i en generisk
    /// database. Der skal dog fastlægges en semantik omkring dette, og det er også 
    /// muligt vi skal vælge at acceptere, at databasen skal nulstilles, når man ændrer
    /// i klassehierarkiet. Det er trods alt ikke afgørende for at undersøge, om man 
    /// kan få skidtet til at performe effektivt.
    /// </summary>
    class TypeSystem
    {
        internal const string COLLECTION_ELEMENT_TYPE_PROPERTY_NAME = "++ElementType"; // prefixed with ++ which is not legal in a C# cstProperty name
        internal const string COLLECTION_ELEMENT_MAPPING_TYPE_PROPERTY_NAME = "++ListElementMappingType"; 
        internal const string COLLECTION_KEY_PROPERTY_NAME = "++KeyType";      // to avoid clashes with existing properties.

        private  Dictionary<long, IETCacheElement> etid2IEt = new Dictionary<long, IETCacheElement>();
        private  Dictionary<string, IETCacheElement> name2IEt = new Dictionary<string, IETCacheElement>();
        private  Dictionary<Type, IETCacheElement> type2IEt = new Dictionary<Type, IETCacheElement>();

        private  Dictionary<long, IPropertyType> ptid2pt = new Dictionary<long, IPropertyType>();
        private  Dictionary<string, IPropertyType> ptName2pt = new Dictionary<string, IPropertyType>();

        private DataContext dataContext;

        internal TypeSystem(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        /// <summary>
        /// After calling this method, all EntityTypes 
        /// from database must be cached in memory. 
        /// 
        /// New EntityTypes must be created through
        /// this typesystem instance which will take care
        /// og persisting entity types, cstProperty types and
        /// properties.
        /// </summary>
        internal void Init()
        {
            foreach (IEntityType ets in dataContext.GenDB.GetAllEntityTypes())
            {
                if (!etid2IEt.ContainsKey(ets.EntityTypePOID))
                {
                    RegisterType(ets);
                }
            }

            foreach (IPropertyType pt in dataContext.GenDB.GetAllPropertyTypes())
            {
                ptid2pt.Add(pt.PropertyTypePOID, pt);
                ptName2pt.Add(pt.Name, pt);
            }

            // Must register transalators after all types has been loaded to 
            // prevent the translator to reregister types.
            foreach(IETCacheElement ets in etid2IEt.Values)
            {
                dataContext.Translators.RegisterTranslator(ets.ClrType, ets.Target);
            }

#if DEBUG
            Console.WriteLine("Type system init done.");
#endif
        }

        /// <summary>
        /// Registers inheritance structure of the given type
        /// and adds it to the database.
        /// 
        /// Stores IEntityType to DB
        /// </summary>
        /// <param name="et"></param>
        internal void RegisterType(IEntityType et)
        {

            if (etid2IEt.ContainsKey(et.EntityTypePOID))
            {
                throw new Exception("Type already registered. (" + et.ToString() + ")");
            }
            if (et.SuperEntityType != null && !etid2IEt.ContainsKey(et.SuperEntityType.EntityTypePOID))
            {
                RegisterType(et.SuperEntityType);
            }

            Assembly assembly = Assembly.Load(et.AssemblyDescription);
            Type t = assembly.GetType(et.Name, true);
            if (t == null) { throw new Exception("Could not find a CLR type with name: " + et.Name); }
            IETCacheElement ce = new IETCacheElement(et, t);
            // Use add to ensure exception, if something is attempted to be inputted twice.
            etid2IEt.Add(et.EntityTypePOID, ce);
            name2IEt.Add(et.Name, ce);
            type2IEt.Add(ce.ClrType, ce);
            
            // Register et at its supertype, if one is present
            if (et.SuperEntityType != null)
            {
                etid2IEt[et.SuperEntityType.EntityTypePOID].AddSubType(et);
            }
            dataContext.GenDB.Save(et);
            dataContext.GenDB.CommitTypeChanges();
            //dataContext.Translators.RegisterTranslator(ce.ClrType, ce.Target);
        }

        /// <summary>
        /// Registers the type for usage internally in the 
        /// translation system and writes the metadescription
        /// to the DB.
        /// </summary>
        /// <param name="t"></param>
        internal void RegisterType(Type t)
        {
            if (type2IEt.ContainsKey(t))
            {
                throw new Exception("Type already registered. (" + t.ToString() + ")");
            }
            //try 
            //{
            TranslatorChecks.CheckIBusinessObjectTranslatability(t);
            IEntityType et = ConstructEntityType(t);
            RegisterType(et);
            dataContext.Translators.RegisterTranslator(t, et);
            //}
            //catch(NotTranslatableException e)
            //{
            //    throw e;
            //}
        }

        /// <summary>
        /// Returns IEntityType with given entityTypePOID.
        /// Since all IEntityTypes are loaded upon instantiation,
        /// this method does not perform any checks to test if
        /// the ID is legal or not.
        /// </summary>
        /// <param name="entityTypePOID"></param>
        /// <returns></returns>
        public  IEntityType GetEntityType(long entityTypePOID)
        {
            return etid2IEt[entityTypePOID].Target;
        }

        public  Type GetClrType(IEntityType et)
        {
            return etid2IEt[et.EntityTypePOID].ClrType;
        }

        /// <summary>
        /// Returns the IEntityType used to describe
        /// a Type internally.
        /// <p/>
        /// Does not check if the Type is recognized
        /// by the TypeSystem. Throws exception if this
        /// is not the case. Use IsTypeKnown to determine
        /// if it is the case. 
        /// <p/>
        /// If Type is unknown the internal method RegisterType 
        /// can be used to make it known to the TypeSystem.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal  IEntityType GetEntityType(Type t)
        {
            return type2IEt[t].Target;
        }

        internal bool IsTypeKnown(Type t)
        {
            return type2IEt.ContainsKey(t);
        }

        /// <summary>
        /// Returns the IEntityType given and all sub entity types.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public  IEnumerable<IEntityType> GetEntityTypesInstanceOf(IEntityType iet)
        {
            // List for storing the result
            LinkedList<IEntityType> result = new LinkedList<IEntityType>();

            // Add the element it self to the list
            result.AddLast(iet);

            //Traverse through all direct sub types
            foreach (IEntityType et in etid2IEt[iet.EntityTypePOID].DirectSubTypes)
            {
                // Add the sub type and all its sub types recursively.
                foreach (IEntityType et2 in GetEntityTypesInstanceOf(et))
                {
                    result.AddLast(et2);
                }
            }
            return result;
        }


        /// <summary>
        /// Gets all known sub types of t. (That is all types written to the
        /// DB, since the TypeSystem is always kept in alignment with the database
        /// as regards types.)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public  IEnumerable<IEntityType> GetEntityTypesInstanceOf(Type t)
        {
            if (!IsTypeKnown(t)) 
            { // Ensure that the type is known. 
                return new IEntityType[0];
            }
            return GetEntityTypesInstanceOf(type2IEt[t].Target);
        }

        /// <summary>
        /// Gets all known sub types of t. (That is all types written to the
        /// DB, since the TypeSystem is always kept in alignment with the database
        /// as regards types.)
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public  IEnumerable<IEntityType> GetEntityTypesInstanceOf(long entityTypePOID)
        {
            return GetEntityTypesInstanceOf(etid2IEt[entityTypePOID].Target);
        }

               
        private IEntityType BOListEntityType(Type clrType)
        {
            IEntityType res = dataContext.GenDB.NewEntityType();
            res.IsList = true;
            res.AssemblyDescription = clrType.Assembly.FullName;
            res.Name = clrType.FullName;

            IPropertyType pt = dataContext.TypeSystem.GetPropertyType(clrType);
            IProperty property = dataContext.GenDB.NewProperty();
            property.EntityType = res;
            property.PropertyName = TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME;
            property.PropertyType = pt;
            res.AddProperty (property);
            if (clrType.BaseType != null)
            {
                if (!IsTypeKnown(clrType.BaseType)) { RegisterType(clrType.BaseType); }
                res.SuperEntityType = GetEntityType(clrType.BaseType);
            }

            return res;
        }

        private IEntityType BODictionaryEntityType(Type clrType)
        {
            IEntityType res = dataContext.GenDB.NewEntityType();
            res.IsDictionary = true;
            res.AssemblyDescription = clrType.Assembly.FullName;
            res.Name = clrType.FullName;
            
            IPropertyType pt = dataContext.TypeSystem.GetPropertyType(clrType);
            IProperty property = dataContext.GenDB.NewProperty();
            property.EntityType = res;
            property.PropertyName = TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME;
            property.PropertyType = pt;
            res.AddProperty (property);

            if (clrType.BaseType != null)
            {
                if (!IsTypeKnown(clrType.BaseType)) { RegisterType(clrType.BaseType); }
                res.SuperEntityType = GetEntityType(clrType.BaseType);
            }
            return res;
        }

        private IEntityType IBOEntityType(Type t)
        {
            IEntityType et = dataContext.GenDB.NewEntityType();

            et.Name = t.FullName;
            et.AssemblyDescription = t.Assembly.FullName;

            PropertyInfo[] clrProperties = t.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                        BindingFlags.Public);

            foreach (PropertyInfo clrProperty in clrProperties)
            {
                Attribute volatileAttribute = Attribute.GetCustomAttribute(clrProperty, typeof(Volatile));
                if (clrProperty.PropertyType != typeof(DBIdentifier) && volatileAttribute == null)
                {
                    IProperty property = dataContext.GenDB.NewProperty();
                    property.PropertyName = clrProperty.Name;
                    property.PropertyType = GetPropertyType(clrProperty.PropertyType);
                    // cstProperty.MappingType = FindMappingType(clrProperty);
                    property.EntityType = et;
                    et.AddProperty(property);
                }
            }

            Type superType = t.BaseType;
            if (superType != null && superType != typeof(object))
            {
                if (type2IEt.ContainsKey(superType))
                {
                    et.SuperEntityType = type2IEt[superType].Target;
                }
                else
                {
                    IEntityType set = ConstructEntityType(superType);
                    RegisterType(set);
                    et.SuperEntityType = set;
                }
            }
            return et;
        }

        public  IEntityType ConstructEntityType(Type t)
        {
            if (t.IsGenericType ) 
            {
                if (t.GetGenericTypeDefinition() == BOListTranslator.TypeOfBOList)
                {
                    return BOListEntityType(t);
                }
                    // dirty stuff, could'n get GetGenericTypeDefinition to work with dictionaries...help me!
                else if(t.Name.Substring(0,6)=="BODict")
                {
                    return BODictionaryEntityType(t);
                }
                else 
                {
                    throw new NotTranslatableException ("Don't know how to construct IEntityType for type", t.GetGenericTypeDefinition());
                }
            }
            else
            {
                return IBOEntityType(t);
            }
        }

        public MappingType FindMappingType(Type t)
        {
            if (t == null) { throw new NullReferenceException("Type t"); }
            if (t.IsEnum)
            {
                return MappingType.LONG;
            }
            else if (t.IsPrimitive)
            {
                if (t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(uint))
                {
                    return MappingType.LONG;
                }
                else if (t == typeof(bool))
                {
                    return MappingType.BOOL;
                }
                else if (t == typeof(char))
                {
                    return MappingType.LONG;
                }
                else if (t == typeof(float) || t == typeof(double))
                {
                    return MappingType.DOUBLE;
                }
                else
                {
                    throw new NotTranslatableException("Did not know how to map primitive Type", t);
                }
            }
            else if (t.IsArray)
            {
                throw new NotTranslatableException("Can not translate arrays.", t);
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
                throw new NotTranslatableException("Can not find mappingtype of type.", t);
            }
        }

        public MappingType FindMappingType(PropertyInfo clrProperty)
        {
            Type t = clrProperty.PropertyType;
            try
            {
                return FindMappingType(t);
            }
            catch (NotTranslatableException e)
            {
                throw new NotTranslatableException("Can not translate Property", clrProperty, e);
            }
        }

        /// <summary>
        /// Will return a PropertyType with the given name. If none exists
        /// prior to calling this method, a new PropertyType will be added 
        /// to the DB automatically.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IPropertyType GetPropertyType(Type t)
        {
            IPropertyType res;
            if (ptName2pt.TryGetValue(t.FullName, out res))
            {
                return res;
            }
            else
            {
                res = dataContext.GenDB.NewPropertyType();
                res.Name = t.FullName;
                res.MappingType = FindMappingType(t);
                ptName2pt.Add(res.Name, res);
                ptid2pt.Add(res.PropertyTypePOID, res);
            }
            return res;
        }
    }

    public static class IBOExtensions
    {
        public static int GetEntityPOID(this IBusinessObject ibo)
        {
            throw new Exception("Hey dummy, this doesn't work!");
        }
    }
}
