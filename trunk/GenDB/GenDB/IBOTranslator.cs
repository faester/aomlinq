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
     */

    delegate void PropertyValueSetter(IEntity e, object value);
    delegate void PropertySetter(IBusinessObject ibo, object value);


    /// <summary>
    /// Translates between IBusinessObject and IEntity. Not type safe, so the 
    /// IBOTranslator should be stored in a hash table with types as key.
    /// (Or be instantiated anew for each type, which is of course less effective
    /// due to instantiation time.)
    /// The IBOTranslator got its name because it uses delegates for translation, 
    /// rather than reflection. Might be misleading.
    /// </summary>
    class IBOTranslator : IIBoToEntityTranslator
    {
        public class UnknownPropertyException : Exception
        {
            int propertyPOID;
            IProperty property;

            internal IProperty Property
            {
                get { return property; }
            }

            public int PropertyPOID
            {
                get { return propertyPOID; }
            }

            public UnknownPropertyException(int propertyPOID)
            {
                this.propertyPOID = propertyPOID;
            }

            public UnknownPropertyException (IProperty property)
            {
                this.propertyPOID = property.PropertyPOID;
                this.property = property;
            }
        }

        IIBoToEntityTranslator superTranslator = null;
        IEntityType entityType;
        DataContext dataContext = null;
        Type t;
        PropertyInfo[] fields;
        LinkedList<PropertyConverter> fieldConverters = new LinkedList<PropertyConverter>();
        LinkedList<PropertyConverter> allFieldConverters = new LinkedList<PropertyConverter>();

        public IEnumerable<PropertyConverter> FieldConverters
        {
            get {
                return allFieldConverters;
            }
        }

        Dictionary<long, PropertyConverter> fieldConverterDict = new Dictionary<long, PropertyConverter>();
        
        InstantiateObjectHandler instantiator;
        private IBOTranslator() { /* empty */ }

        public IEntityType EntityType
        {
            get { return entityType; }
        }

        public IBusinessObject CreateInstanceOfIBusinessObject()
        {
            return (IBusinessObject)instantiator();
        }

        public void SetProperty(long propertyPOID, IBusinessObject obj, object propertyValue)
        {
            fieldConverterDict[propertyPOID].PropertySetter(obj, propertyValue);
        }

        public IBOTranslator(Type t, IEntityType iet, DataContext dataContext)
        {
            if (dataContext == null) { throw new NullReferenceException("TypeSystem"); }
            if (iet == null) { throw new NullReferenceException("IEntityType"); }

            this.dataContext = dataContext;
            this.entityType = iet;
            this.t = t;

            Init();
        }

        public void SaveToDB(IGenericDatabase db, IBusinessObject ibo)
        {
            IEntity e = this.Translate(ibo);
            db.Save(e);
        }

        public PropertyConverter GetPropertyConverter(int propertyPOID)
        {
            try {
                return this.fieldConverterDict[propertyPOID];
            }
            catch(KeyNotFoundException)
            {
                throw new UnknownPropertyException(propertyPOID);
            }
        }

        public PropertyConverter GetPropertyConverter(IProperty property)
        {
            try {
            return GetPropertyConverter(property.PropertyPOID);
            }
            catch(UnknownPropertyException)
            {
                throw new UnknownPropertyException(property);
            }
        }


        private void Init()
        {
            CheckTranslatability();
            SetPropertyInfo();
            InitPropertyTranslators();
            InitInstantiator();
            InitSuperTranslator();
            foreach(PropertyConverter fc in fieldConverterDict.Values)
            {
                allFieldConverters.AddLast(fc);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitSuperTranslator()
        {
            if (entityType.SuperEntityType != null)
            {
                superTranslator = dataContext.Translators.GetTranslator(entityType.SuperEntityType.EntityTypePOID);
                foreach (PropertyConverter fc in superTranslator.FieldConverters)
                {
                    fieldConverterDict[fc.PropertyPOID] = fc;
                }
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
            foreach (PropertyInfo clrProperty in fields)
            {
                Attribute a = Volatile.GetCustomAttribute(clrProperty, typeof(Volatile));
                if (clrProperty.PropertyType != typeof(DBIdentifier) && a == null)
                {
                    IProperty prop = this.entityType.GetProperty(clrProperty.Name);
                    fieldConverters.AddLast(new PropertyConverter(t, clrProperty, prop, dataContext));
                    fieldConverterDict[prop.PropertyPOID] = fieldConverters.Last.Value;
                    if (
                        TranslatorChecks.ImplementsIBusinessObject(clrProperty.PropertyType)
                        && !dataContext.TypeSystem.IsTypeKnown(clrProperty.PropertyType)
                        )
                    {
                        dataContext.TypeSystem.RegisterType(clrProperty.PropertyType);
                    }
                }
            }
        }

        private void InitInstantiator()
        {
            instantiator = DynamicMethodCompiler.CreateInstantiateObjectHandler(t);
        }

        public IEntity Translate(IBusinessObject ibo)
        {
            IEntity res = dataContext.GenDB.NewEntity();

            // Drop the db-created DBIdentity if DBTag is set.
            if (ibo.DBIdentity.IsPersistent)
            {
                res.EntityPOID = ibo.DBIdentity;
            }
            else
            { // No DBTag. Add it to cache/db, and assign tag
                dataContext.IBOCache.Add(ibo, res.EntityPOID);
            }
            res.EntityType = entityType;
            SetValues(ibo, res);
            return res;
        }

        public void SetValues(IBusinessObject ibo, IEntity e)
        {
            // Append fields defined at this entity type in the object hierarchy
            if (entityType.DeclaredProperties != null)
            {
                foreach (IProperty property in entityType.DeclaredProperties)
                {
                    IPropertyValue propertyValue = property.CreateNewPropertyValue (e);
                }

                foreach (PropertyConverter fcv in fieldConverters)
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
}