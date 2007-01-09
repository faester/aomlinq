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
    /// Aktuelt skaber det Exceptions, hvis der ændres i typer i en eksisterende DB.
    /// Det skal formentlig udbedres, da det i nogen grad fjerner ideen i en generisk
    /// database. Der skal dog fastlægges en semantik omkring dette, og det er også 
    /// muligt vi skal vælge at acceptere, at databasen skal nulstilles, når man ændrer
    /// i klassehierarkiet. Det er trods alt ikke afgørende for at undersøge, om man 
    /// kan få skidtet til at performe effektivt.
    /// </summary>
    static class TypeSystem
    {
        internal const string COLLECTION_ELEMENT_TYPE_PROPERTY_NAME = "++ElementType"; // prefixed with ++ which is not legal in a C# property name
        internal const string COLLECTION_KEY_PROPERTY_NAME = "++KeyType";      // to avoid clashes with existing properties.

        private static Dictionary<long, IETCacheElement> etid2IEt = new Dictionary<long, IETCacheElement>();
        private static Dictionary<string, IETCacheElement> name2IEt = new Dictionary<string, IETCacheElement>();
        private static Dictionary<Type, IETCacheElement> type2IEt = new Dictionary<Type, IETCacheElement>();

        private static Dictionary<long, IPropertyType> ptid2pt = new Dictionary<long, IPropertyType>();
        private static Dictionary<string, IPropertyType> ptName2pt = new Dictionary<string, IPropertyType>();

        static TypeSystem()
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
        private static void Init()
        {
            foreach (IEntityType ets in Configuration.GenDB.GetAllEntityTypes())
            {
                RegisterType(ets);
            }

            foreach (IPropertyType pt in Configuration.GenDB.GetAllPropertyTypes())
            {
                ptid2pt.Add(pt.PropertyTypePOID, pt);
                ptName2pt.Add(pt.Name, pt);
            }

            foreach (IETCacheElement ce in etid2IEt.Values)
            {
                ce.InitTranslator();
            }
#if DEBUG
            Console.WriteLine("Type system init done.");
#endif
        }

        /// <summary>
        /// Registers inheritance structure of the given type
        /// and adds it to the database.
        /// </summary>
        /// <param name="et"></param>
        internal static void RegisterType(IEntityType et)
        {
            if (etid2IEt.ContainsKey(et.EntityTypePOID))
            {
                return;
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
            
            //// We need the type to appear to be known prior to instantiating the 
            //// translator. Otherwise recursive data structures migth cause infinite loop.
            //ce.InitTranslator();

            // Register et at its supertype, if one is present
            if (et.SuperEntityType != null)
            {
                etid2IEt[et.SuperEntityType.EntityTypePOID].AddSubType(et);
            }
            Configuration.GenDB.Save(et);
            Configuration.GenDB.CommitTypeChanges();
        }

        /// <summary>
        /// Registers the type for usage internally in the 
        /// translation system and writes the metadescription
        /// to the DB.
        /// </summary>
        /// <param name="t"></param>
        internal static void RegisterType(Type t)
        {
            if (!type2IEt.ContainsKey(t))
            {
                IEntityType et = ConstructEntityType(t);
                RegisterType(et);
                Configuration.GenDB.Save(et) ; //TODO: Is this needed?
                etid2IEt[et.EntityTypePOID].InitTranslator();
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
        public static IEntityType GetEntityType(long entityTypePOID)
        {
            return etid2IEt[entityTypePOID].Target;
        }

        public static Type GetClrType(IEntityType et)
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
        internal static IEntityType GetEntityType(Type t)
        {
            return type2IEt[t].Target;
        }

        internal static IIBoToEntityTranslator GetTranslator(Type t)
        {
            return type2IEt[t].Translator;
        }

        internal static IIBoToEntityTranslator GetTranslator(long entityTypePOID)
        {
            return etid2IEt[entityTypePOID].Translator;
        }

        internal static bool IsTypeKnown(Type t)
        {
            return type2IEt.ContainsKey(t);
        }

        /// <summary>
        /// Returns the IEntityType given and all sub entity types.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static IEnumerable<IEntityType> GetEntityTypesInstanceOf(IEntityType iet)
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
        public static IEnumerable<IEntityType> GetEntityTypesInstanceOf(Type t)
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
        public static IEnumerable<IEntityType> GetEntityTypesInstanceOf(long entityTypePOID)
        {
            return GetEntityTypesInstanceOf(etid2IEt[entityTypePOID].Target);
        }

        public static IEntityType ConstructEntityType(Type t)
        {
            if (t.IsGenericType ) // TODO: Needs better checking
            {
                if (t.GetGenericTypeDefinition() == BOListTranslator.TypeOfBOList)
                {
                    IIBoToEntityTranslator trans = Translators.GetTranslator(t, null);
                    return trans.EntityType;
                }
                else 
                {
                    throw new NotTranslatableException ("Don't know how to construct IEntityType for type", t);
                }
            }
            else
            {
                IEntityType et = Configuration.GenDB.NewEntityType();

                et.Name = t.FullName;
                et.AssemblyDescription = t.Assembly.FullName;

                PropertyInfo[] clrProperties = t.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                            BindingFlags.Public);

                foreach (PropertyInfo clrProperty in clrProperties)
                {
                    Attribute volatileAttribute = Attribute.GetCustomAttribute(clrProperty, typeof(Volatile));
                    if (clrProperty.PropertyType != typeof(DBTag) && volatileAttribute == null)
                    {
                        IProperty property = Configuration.GenDB.NewProperty();
                        property.PropertyName = clrProperty.Name;
                        property.PropertyType = GetPropertyType(clrProperty.PropertyType.FullName);
                        property.MappingType = FindMappingType(clrProperty);
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
        }

        public static MappingType FindMappingType(Type t)
        {
            if (t == null) { throw new NullReferenceException("Type t"); }
            if (t.IsEnum)
            {
                return MappingType.LONG;
            }
            else if (t.IsPrimitive)
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

        public static MappingType FindMappingType(PropertyInfo clrProperty)
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
        public static IPropertyType GetPropertyType(string name)
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
                ptName2pt.Add(res.Name, res);
                ptid2pt.Add(res.PropertyTypePOID, res);
            }
            return res;
        }
    }
}
