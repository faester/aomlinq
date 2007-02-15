using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;
using System.Reflection;

namespace GenDB
{
    abstract class BaseTranslator :  IIBoToEntityTranslator
    {
        IEntityType entityType;

        protected InstantiateObjectHandler instantiator;
        protected LinkedList<PropertyConverter> fieldConverters = new LinkedList<PropertyConverter>();
        protected LinkedList<PropertyConverter> allFieldConverters = new LinkedList<PropertyConverter>();

        protected Dictionary<long, PropertyConverter> fieldConverterDict = new Dictionary<long, PropertyConverter>();

        protected IIBoToEntityTranslator superTranslator = null;
        protected DataContext dataContext = null;
        protected Type t;
        protected PropertyInfo[] fields;

        public BaseTranslator(Type t, IEntityType iet, DataContext dataContext)
        {
            if (dataContext == null) { throw new NullReferenceException("dataContext"); }
            if (iet == null) { throw new NullReferenceException("IEntityType"); }

            this.dataContext = dataContext;
            this.entityType = iet;
            this.t = t;
            Init();
        }
        
        public IEntityType EntityType
        {
            get { return entityType; }
        }

        public IBusinessObject CreateInstanceOfIBusinessObject()
        {
            return (IBusinessObject)instantiator();
        }


        public IEnumerable<PropertyConverter> FieldConverters
        {
            get
            {
                return allFieldConverters;
            }
        }


        protected void Init()
        {
            fields = GetPropertiesToTranslate();
            GetPropertiesToTranslate();
            InitPropertyTranslators();
            InitInstantiator();
            InitSuperTranslator();
            foreach (PropertyConverter fc in fieldConverterDict.Values)
            {
                allFieldConverters.AddLast(fc);
            }
        }

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

        public abstract void SetProperty(long propertyPOID, IBusinessObject obj, object propertyValue);

        public abstract void SaveToDB(IBusinessObject ibo);


        public PropertyConverter GetPropertyConverter(int propertyPOID)
        {
            try
            {
                return this.fieldConverterDict[propertyPOID];
            }
            catch (KeyNotFoundException)
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

        protected abstract PropertyInfo[] GetPropertiesToTranslate();


        
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
                    IPropertyValue propertyValue = property.CreateNewPropertyValue(e);
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
