using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Query;
using GenDB.DB;

namespace GenDB
{
    /*
     * http://www.codeproject.com/csharp/delegates_and_reflection.asp
     * http://www.codeproject.com/useritems/Dynamic_Code_Generation.asp
     * 
     * Udelad PropertyType og brug kun enum. 
     */
    delegate object PropertyValueGetter(IEntity e);
    delegate void PropertyValueSetter(IEntity e, object value);

    /// <summary>
    /// Translates between IBusinessObject and IEntity. Not type safe, so the 
    /// IBOTranslator should be stored in a hash table with types as key.
    /// (Or be instantiated anew for each type, which is of course ineffective
    /// due to instantiation time.)
    /// The IBOTranslator got its name because it uses delegates for translation, 
    /// rather than reflection. Might be misleading.
    /// </summary>
    class IBOTranslator : IIBoToEntityTranslator
    {
        IIBoToEntityTranslator superTranslator = null;
        IEntityType iet;
        Type t;
        PropertyInfo[] fields;
        LinkedList<FieldConverter> fieldConverters = new LinkedList<FieldConverter>();
        InstantiateObjectHandler instantiator;
        private IBOTranslator() { /* empty */ }

        public IBOTranslator(Type t, IEntityType iet)
        {
            this.iet = iet;
            this.t = t;
            Init();
        }

        private void Init()
        {
            CheckTranslatability();
            SetPropertyInfo();
            InitPropertyTranslators();
            InitInstantiator();
            InitSuperTranslator();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitSuperTranslator()
        {
            if (iet.SuperEntityType != null)
            {
                superTranslator = TypeSystem.GetTranslator(iet.SuperEntityType.EntityTypePOID);
            }
        }

        /// <summary>
        /// Stores the PropertyInfo array of fields to translate.
        /// </summary>
        private void SetPropertyInfo()
        {
            fields = t.GetProperties(
                BindingFlags.Public
                | BindingFlags.DeclaredOnly
                | BindingFlags.Instance
                );
        }

        /// <summary>
        /// Checks if Type and fields are translatable.
        /// </summary>
        private void CheckTranslatability()
        {
            TranslatorChecks.CheckObjectTypeTranslateability(t);
            PropertyInfo[] allFields = t.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Static
                | BindingFlags.Public | BindingFlags.Instance);
            TranslatorChecks.CheckPropertyTranslatability(allFields);
        }

        private void InitPropertyTranslators()
        {
            Dictionary<string, IProperty> properties =
                iet.GetAllProperties.ToDictionary((IProperty p) => p.PropertyName);

            foreach (PropertyInfo clrProperty in fields)
            {
                Attribute a = Volatile.GetCustomAttribute (clrProperty, typeof(Volatile));
                if (clrProperty.PropertyType != typeof(DBTag) && a == null)
                {
                    IProperty prop = properties[clrProperty.Name];
                    fieldConverters.AddLast(new FieldConverter(t, clrProperty, prop));
                }
            }
        }

        private void InitInstantiator()
        {
            instantiator = DynamicMethodCompiler.CreateInstantiateObjectHandler(t);
        }

        /// <summary>
        /// Translates the IEntity given to a IBusinessObject 
        /// instance. If the cache contains a business object 
        /// with an id edentical to e.EntityPOID the cached 
        /// businessobject will be returned, regardless of the 
        /// PropertyValues in e.
        /// <p/> 
        /// TODO: Consider if the responsibility of cache checking 
        /// and object substitution should happen in the Table 
        /// class instead.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        
        public IBusinessObject Translate(IEntity e)
        {
            IBusinessObject res = IBOCache.Get(e.EntityPOID);
            if (res == null)
            {
                res = (IBusinessObject)instantiator();
                SetValues(e, res);
                DBTag.AssignDBTagTo(res, e.EntityPOID);
            }
            return res;
        }

        /// <summary>
        /// Copies values from PropertyValues stored in 
        /// e to the fields in ibo. (Thus changing state 
        /// of ibo)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ibo"></param>
        public void SetValues(IEntity e, IBusinessObject ibo)
        {
            foreach (FieldConverter c in fieldConverters)
            {
                c.SetObjectFieldValue(ibo, e);
            }
            if (superTranslator != null)
            {
                superTranslator.SetValues(e, ibo);
            }
        }

        public IEntity Translate(IBusinessObject ibo)
        {
            IEntity res = Configuration.GenDB.NewEntity();
            // Drop the db-created EntityPOID if DBTag is set.
            if (ibo.DBTag != null)
            {
                res.EntityPOID = ibo.DBTag.EntityPOID;
            }
            else
            { // No DBTag. Add it to cache/db, and assign tag
                DBTag.AssignDBTagTo(ibo, res.EntityPOID);
            }
            res.EntityType = iet;
            SetValues(ibo, res);
            return res;
        }

        public void SetValues(IBusinessObject ibo, IEntity e)
        {
            // Append fields defined at this entity type in the object hierarchy
            if (iet.DeclaredProperties != null)
            {
                foreach (IProperty property in iet.DeclaredProperties)
                {
                    IPropertyValue propertyValue = Configuration.GenDB.NewPropertyValue();
                    propertyValue.Entity = e;
                    propertyValue.Property = property;
                    e.StorePropertyValue(propertyValue);
                }

                foreach (FieldConverter fcv in fieldConverters)
                {
                    fcv.SetEntityPropertyValue(ibo, e);
                }
            }

            // Test if we have a super type (translator), and apply if it is the case
            if (superTranslator != null)
            {
                superTranslator.SetValues(ibo, e);
            }
        }
    }

    class FieldConverter
    {
        PropertyValueGetter pvg;
        PropertyValueSetter pvs;
        SetHandler sh;
        GetHandler gh;
        Type clrType;
        PropertyInfo fi;

        public FieldConverter(Type t, PropertyInfo fi, IProperty property)
        {
            this.fi = fi;
            clrType = t;
            sh = DynamicMethodCompiler.CreateSetHandler(t, fi);
            gh = DynamicMethodCompiler.CreateGetHandler(t, fi);
            pvg = CreateGetter(fi, property);
            pvs = CreateSetter(property);
        }

        public void SetObjectFieldValue(IBusinessObject ibo, IEntity entity)
        {
            object value = pvg(entity);
            sh(ibo, value);
        }

        public void SetEntityPropertyValue(IBusinessObject ibo, IEntity e)
        {
            pvs(e, gh(ibo));
        }

        /// <summary>
        /// TODO: Change the GetHandler etc to use primitives and objects.
        /// (Should be GetLongHandler etc..)
        /// </summary>
        /// <param name="clrProperty"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        private PropertyValueGetter CreateGetter(PropertyInfo fi, IProperty prop)
        {
            if ((fi.PropertyType.IsByRef || fi.PropertyType.IsClass) 
                && !(fi.PropertyType == typeof(string) || fi.PropertyType == typeof(DateTime))
                )
            { // Handles references other than string and DateTime
                return delegate(IEntity ie)
                {
                    IBOReference entityRef = (IBOReference)(ie.GetPropertyValue(prop).RefValue);
                    if (!entityRef.IsNullReference)
                    {
                        IBusinessObject res = IBOCache.Get(entityRef.EntityPOID);
                        if (res == null)
                        {
                            IEntity e = Configuration.GenDB.GetEntity(entityRef.EntityPOID);
                            IIBoToEntityTranslator trans = TypeSystem.GetTranslator(clrType);
                            res = trans.Translate(e);
                        }
                        return res;
                    }
                    else
                    {
                        return null;
                    }
                };
            }
            if (fi.PropertyType == typeof(long))
            {
                return delegate(IEntity ie)
                {
                    return ie.GetPropertyValue(prop).LongValue;
                };
            }
            else if (fi.PropertyType == typeof(int))
            {
                return delegate(IEntity ie) { return (int)ie.GetPropertyValue(prop).LongValue; };
            }
            else if (fi.PropertyType == typeof(string))
            {
                return delegate(IEntity ie)
                {
                    return ie.GetPropertyValue(prop).StringValue;
                };
            }
            else if (fi.PropertyType == typeof(DateTime))
            {
                return delegate(IEntity ie)
                {
                    return ie.GetPropertyValue(prop).DateTimeValue;
                };
            }
            else if (fi.PropertyType == typeof(bool))
            {
                return delegate(IEntity ie) { return ie.GetPropertyValue(prop).BoolValue; };
            }
            else if (fi.PropertyType == typeof(char))
            {
                return delegate(IEntity ie) {  //
                    return ie.GetPropertyValue(prop).CharValue; 
                };
            }
            else if (fi.PropertyType == typeof(float))
            {
                return delegate(IEntity ie)
                {  //
                    return Convert.ToSingle(ie.GetPropertyValue(prop).DoubleValue);
                };
            }
            else if (fi.PropertyType == typeof(double))
            {
                return delegate(IEntity ie)
                {  //
                    return ie.GetPropertyValue(prop).DoubleValue;
                };
            }
            else if (fi.PropertyType.IsEnum)
            {
                return delegate (IEntity ie)
                {
                    return Enum.Parse(fi.PropertyType,ie.GetPropertyValue(prop).StringValue);
                };
            }
            else 
            {
                throw new NotTranslatableException("Have not implemented PropertyValueGetter for field type.", fi);
            }
        }

        PropertyValueSetter CreateSetter(IProperty p)
        {
            switch (p.MappingType)
            {
                case MappingType.BOOL:
                    return delegate(IEntity e, object value) {
                        e.GetPropertyValue(p).BoolValue = Convert.ToBoolean(value);
                    };
                case MappingType.DATETIME:
                    return delegate(IEntity e, object value)
                    {
                        e.GetPropertyValue(p).DateTimeValue = Convert.ToDateTime(value);
                    };
                case MappingType.DOUBLE: 
                    return delegate(IEntity e, object value)
                    { 
                        e.GetPropertyValue(p).DoubleValue = Convert.ToDouble(value);
                    };
                case MappingType.LONG:
                    return delegate(IEntity e, object value)
                    {
                        e.GetPropertyValue(p).LongValue = Convert.ToInt64(value);
                    };
                case MappingType.REFERENCE:
                    return delegate(IEntity e, object value)
                    {
                        if (value == null)
                        {
                            IBOReference reference = new IBOReference(true);
                            e.GetPropertyValue(p).RefValue = reference;
                            return;
                        }
                        IBusinessObject ibo = (IBusinessObject)value;
                        if (ibo.DBTag == null)
                        {
                            // TODO: Do a lot of checking.... :(
                            // Is it safe not to perform any real translation here??
                            IEntity refered = Configuration.GenDB.NewEntity();
                            DBTag.AssignDBTagTo(ibo, refered.EntityPOID);
                            IBOReference reference = new IBOReference (ibo.DBTag.EntityPOID);
                            e.GetPropertyValue(p).RefValue = reference;
                        }
                        else
                        {
                            IBOReference reference = new IBOReference(false, ibo.DBTag.EntityPOID);
                            e.GetPropertyValue(p).RefValue = reference;
                        }
                    };
                case MappingType.STRING:
                    return delegate(IEntity e, object value)
                    {
                        e.GetPropertyValue(p).StringValue = Convert.ToString(value);
                    };
                case MappingType.CHAR: return 
                    delegate(IEntity e, object value) { 
                        e.GetPropertyValue(p).CharValue = Convert.ToChar (value);
                    };
                default:
                    throw new Exception("Unknown MappingType in DelegateTranslator, CreateSetter: " + p.MappingType);
            }
        }
    }
}
