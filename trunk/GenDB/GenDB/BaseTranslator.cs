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
        protected LinkedList<IPropertyConverter> fieldConverters = new LinkedList<IPropertyConverter>();
        protected LinkedList<IPropertyConverter> allFieldConverters = new LinkedList<IPropertyConverter>();

        protected Dictionary<long, IPropertyConverter> fieldConverterDict = new Dictionary<long, IPropertyConverter>();

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


        public IEnumerable<IPropertyConverter> FieldConverters
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
            foreach (IPropertyConverter fc in fieldConverterDict.Values)
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


        public IPropertyConverter GetPropertyConverter(int propertyPOID)
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

        public IPropertyConverter GetPropertyConverter(IProperty property)
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

        private IPropertyConverter ConstructLazyPropertyConverter(
            Type t, 
            PropertyInfo clrProperty, 
            IProperty prop, 
            DataContext dataContext,
            LazyLoad laz
            )
        {
            string fieldName = laz.Storage;
            if (fieldName == null) { throw new NullReferenceException("When Properties are decorated with LazyLoad attribute, the storage field must be specified using 'Storage'-parameter"); }
            FieldInfo field = t.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
            if (field == null) { throw new Exception("Could not find specified storage field '" + fieldName + "'"); }

            Type fieldsType = field.FieldType;
            if (!fieldsType.IsGenericType) { throw new Exception("Storage field is not generic"); }
            Type genericTypeParam = fieldsType.GetGenericArguments()[0];

            if (fieldsType.GetGenericTypeDefinition () != typeof(LazyLoader<>))
            {
                throw new Exception("Storage field must be of type " + typeof(LazyLoader).ToString());
            }

            if (genericTypeParam.GetInterface(typeof(IBusinessObject).ToString()) == null)
            {
                throw new Exception("Storage fields generic type parameter does not implement IBusinessObject.");
            }

            
            return new PropertyConverterLazy(t, field, prop, dataContext, clrProperty);
        }

        private void InitPropertyTranslators()
        {
            foreach (PropertyInfo clrProperty in fields)
            {
                Attribute volatileAttribute = Volatile.GetCustomAttribute(clrProperty, typeof(Volatile));
                Attribute lazyLoadAttribute = Attribute.GetCustomAttribute(clrProperty, typeof(LazyLoad));

                if (clrProperty.PropertyType != typeof(DBIdentifier) && volatileAttribute == null)
                {
                    IProperty prop = this.entityType.GetProperty(clrProperty.Name);
                    if (lazyLoadAttribute != null)
                    {
                        LazyLoad laz = (LazyLoad)lazyLoadAttribute;
                        fieldConverters.AddLast(ConstructLazyPropertyConverter(t, clrProperty, prop, dataContext, laz));
                    }
                    else
                    {
                        fieldConverters.AddLast(new PropertyConverter(t, clrProperty, prop, dataContext));
                    }
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
            // Drop the db-created DBIdentity if DBTag is set.
            if (!ibo.DBIdentity.IsPersistent)
            { // No DBTag. Add it to cache/db, and assign tag

                dataContext.IBOCache.Add(ibo, this.dataContext.GenDB.NextEntityPOID);
            }
            IEntity res = new Entity();
            res.EntityPOID = ibo.DBIdentity;
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

                foreach (IPropertyConverter fcv in fieldConverters)
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
