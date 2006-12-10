using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Query;

namespace GenDB
{
    /*
     * http://www.codeproject.com/csharp/delegates_and_reflection.asp
     * http://www.codeproject.com/useritems/Dynamic_Code_Generation.asp
     * 
     * Mangler setter-metode til PropertyValue. Udelad PropertyType og brug 
     * kun enum. Gem diskriminator i Property i stedet.
     */
    delegate object PropertyValueGetter(IEntity e);
    delegate void PropertyValueSetter(IEntity e, object value);

    static class TranslatorChecks
    {
        static string IBO_NAME = typeof(IBusinessObject).FullName;

        public static void CheckObjectTypeTranslateability(Type t)
        {
            Type hasIBO = t.GetInterface(IBO_NAME);
            if (hasIBO == null) { throw new NotTranslatableException("Reference types must implement IBusinessObject.", t); }
        }

        public static void CheckRefFieldTranslatability(FieldInfo fi)
        {
            if (fi.FieldType == typeof(string)) { /* ok */ }
            else if (fi.FieldType == typeof(DateTime)) { /* ok */ }
            else
            {
                Type hasIBO = fi.FieldType.GetInterface(IBO_NAME);
                if (hasIBO == null) { throw new NotTranslatableException("Reference type fields must implement IBusinessObject", fi); }
            }
        }

        public static void CheckFieldTranslatability(FieldInfo[] fields)
        {
            foreach (FieldInfo fi in fields)
            {
                if (fi.IsStatic) { throw new NotTranslatableException("Can not translate static fields.", fi); }
                if (fi.FieldType.IsArray) { throw new NotTranslatableException("Can not translate arrays.", fi); }
                if (fi.FieldType.IsByRef) { CheckRefFieldTranslatability(fi); }
            }
        }
    }

    /// <summary>
    /// Translates between IBusinessObject and IEntity. Not type safe, so the 
    /// DelegateTranslator should be stored in a hash table with types as key.
    /// (Or be instantiated anew for each type, which is of course ineffective
    /// due to instantiation time.)
    /// The DelegateTranslator got its name because it uses delegates for translation, 
    /// rather than reflection. Might be misleading.
    /// </summary>
    class DelegateTranslator
    {
        IEntityType iet;
        Type t;
        FieldInfo[] fields;
        LinkedList<FieldConverter> fieldConverters = new LinkedList<FieldConverter>();
        InstantiateObjectHandler instantiator;
        private DelegateTranslator() { /* empty */ }

        public DelegateTranslator(Type t, IEntityType iet)
        {
            this.iet = iet;
            this.t = t;
            Init();
        }

        private void Init()
        {
            CheckTranslatability();
            SetFieldInfo();
            InitFieldTranslators();
            InitInstantiator();
        }

        /// <summary>
        /// Stores the FieldInfo array of fields to translate.
        /// </summary>
        private void SetFieldInfo()
        {
            fields = t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public
                | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Checks if Type and fields are translatable.
        /// </summary>
        private void CheckTranslatability()
        {
            TranslatorChecks.CheckObjectTypeTranslateability(t);
            FieldInfo[] allFields = t.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static
                | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            TranslatorChecks.CheckFieldTranslatability(allFields);
        }

        private void InitFieldTranslators()
        {
            Dictionary<string, IProperty> properties =
                iet.GetAllProperties.ToDictionary((IProperty p) => p.PropertyName);

            foreach (FieldInfo fi in fields)
            {
                if (fi.FieldType != typeof(DBTag))
                {
                    IProperty prop = properties[fi.Name];
                    fieldConverters.AddLast(new FieldConverter(t, fi, prop));
                }
            }
        }

        private void InitInstantiator()
        {
            instantiator = DynamicMethodCompiler.CreateInstantiateObjectHandler(t);
        }

        public IBusinessObject Translate(IEntity e)
        {
            IBusinessObject res = (IBusinessObject)instantiator();
            foreach (FieldConverter c in fieldConverters)
            {
                c.SetObjectFieldValue(res, e);
            }
            return res;
        }

        public IEntity Translate(IBusinessObject ibo)
        {
            IEntity res = Configuration.GenDB.NewEntity();
            // Drop the db-created EntityPOID if DBTag is set.
            if (ibo.DBTag != null)
            {
                res.EntityPOID = ibo.DBTag.EntityPOID;
            }
            res.EntityType = iet;
            
            foreach (IProperty property in iet.GetAllProperties)
            {
                IPropertyValue propertyValue = Configuration.GenDB.NewPropertyValue();
                propertyValue.Entity = res;
                propertyValue.Property = property;
                res.StorePropertyValue (propertyValue);
            }

            foreach (FieldConverter fcv in fieldConverters)
            {
                fcv.SetEntityPropertyValue (ibo, res);
            }
            return res;
        }
    }

    class FieldConverter
    {
        PropertyValueGetter pvg;
        PropertyValueSetter pvs;
        SetHandler sh;
        GetHandler gh;
        Type clrType;

        public FieldConverter(Type t, FieldInfo fi, IProperty property)
        {
            clrType = t;
            sh = DynamicMethodCompiler.CreateSetHandler(t, fi);
            gh = DynamicMethodCompiler.CreateGetHandler(t, fi);
            pvg = CreateGetter(fi, property);
            pvs = CreateSetter(property);
        }

        public void SetObjectFieldValue(IBusinessObject ibo, IEntity entity)
        {
            sh(ibo, pvg(entity));
        }

        public void SetEntityPropertyValue(IBusinessObject ibo, IEntity e)
        {
            pvs(e, gh(ibo));
        }

        /// <summary>
        /// TODO: Change the GetHandler etc to use primitives and objects.
        /// (Should be GetLongHandler etc..)
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        private PropertyValueGetter CreateGetter(FieldInfo fi, IProperty prop)
        {
            if (fi.FieldType.IsByRef && !(fi.FieldType == typeof(string) || fi.FieldType == typeof(DateTime)))
            { // Handles references other than string and DateTime
                return delegate(IEntity ie)
                {

                    IBOReference entityRef = (IBOReference)(gh(ie.GetPropertyValue(prop).RefValue));
                    if (!entityRef.IsNullReference)
                    {
                        IBusinessObject res = IBOCache.Instance.Get(entityRef.EntityPOID);
                        if (res == null)
                        {
                            IEntity e = Configuration.GenDB.GetEntity(entityRef.EntityPOID);
                            DelegateTranslator trans = TypeSystem.Instance.GetTranslator(clrType);
                            DBTag.AssignDBTagTo(res, e.EntityPOID, IBOCache.Instance);
                        }
                        return res;
                    }
                    else 
                    {
                        return null;
                    }
                };

            }
            if (fi.FieldType == typeof(long))
            {
                return delegate(IEntity ie) { return gh(ie.GetPropertyValue(prop).LongValue); };
            }
            else if (fi.FieldType == typeof(int))
            {
                return delegate(IEntity ie) { return gh(ie.GetPropertyValue(prop).IntValue); };
            }
            else if (fi.FieldType == typeof(string))
            {
                return delegate(IEntity ie) { 
                    return gh(ie.GetPropertyValue(prop).StringValue); 
                };
            }
            else if (fi.FieldType == typeof(DateTime))
            {
                return delegate(IEntity ie) { 
                    return gh(ie.GetPropertyValue(prop).DateTimeValue); 
                };
            }
            else if (fi.FieldType == typeof(bool))
            {
                return delegate(IEntity ie) { return gh(ie.GetPropertyValue(prop).BoolValue); };
            }
            else if (fi.FieldType == typeof(char))
            {
                return delegate(IEntity ie) { return gh(ie.GetPropertyValue(prop).CharValue); };
            }
            else
            {
                throw new NotTranslatableException("Have not implemented PropertyValueGetter for field type.", fi);
            }
        }

        PropertyValueSetter CreateSetter(IProperty p)
        {
            if (p.MappingType == MappingType.BOOL)
            {
                return delegate(IEntity e, object value) { e.GetPropertyValue(p).BoolValue = (bool)value; };
            }
            else if (p.MappingType == MappingType.DATETIME)
            {
                return delegate(IEntity e, object value) { 
                    e.GetPropertyValue(p).DateTimeValue = (DateTime)value; 
                };
            }
            else if (p.MappingType == MappingType.DOUBLE)
            {
                return delegate(IEntity e, object value) { e.GetPropertyValue(p).DoubleValue = (double)value; };
            }
            else if (p.MappingType == MappingType.LONG)
            {
                return delegate(IEntity e, object value) {
                    Type t  = value.GetType ();
                    long v = (int)value;
                    IPropertyValue pv = e.GetPropertyValue(p);
                    pv.LongValue = v;
                };
            }
            else if (p.MappingType == MappingType.REFERENCE)
            {
                return delegate(IEntity e, object value) {
                    if (value == null)
                    {
                        IBOReference reference = new IBOReference (true);
                        e.GetPropertyValue(p).RefValue = reference;
                        return;
                    }
                    IBusinessObject ibo = (IBusinessObject) value;
                    if (ibo.DBTag == null)
                    {
                        // TODO: Do a lot of checking.... :(
                        // Is it safe not to perform any real translation here??
                        IEntity refered = Configuration.GenDB.NewEntity();
                        DBTag.AssignDBTagTo(ibo, refered.EntityPOID, IBOCache.Instance);
                        IBOReference reference = e.GetPropertyValue(p).RefValue;
                        e.GetPropertyValue(p).RefValue = reference;
                    }
                    else
                    {
                        IBOReference reference = new IBOReference(false, ibo.DBTag.EntityPOID);
                        e.GetPropertyValue(p).RefValue = reference;
                    }
                };
            }
            else if (p.MappingType == MappingType.STRING)
            {
                return delegate(IEntity e, object value) 
                { 
                    e.GetPropertyValue(p).StringValue = value.ToString(); 
                };
            }
            else
            {
                throw new Exception("Unknown MappingType in DelegateTranslator, CreateSetter: " + p.MappingType);
            }
        }
    }


}
