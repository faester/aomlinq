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
    delegate object PropertyValueGetter(IEntity e);
    delegate void PropertyValueSetter(IEntity e, object value);


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
        IIBoToEntityTranslator superTranslator = null;
        IEntityType entityType;
        DataContext dataContext = null;
        Type t;
        PropertyInfo[] fields;
        LinkedList<FieldConverter> fieldConverters = new LinkedList<FieldConverter>();
        InstantiateObjectHandler instantiator;
        private IBOTranslator() { /* empty */ }

        public IEntityType EntityType
        {
            get { return entityType; }
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
            if (entityType.SuperEntityType != null)
            {
                superTranslator = dataContext.Translators.GetTranslator(entityType.SuperEntityType.EntityTypePOID);
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
                entityType.GetAllProperties.ToDictionary((IProperty p) => p.PropertyName);

            foreach (PropertyInfo clrProperty in fields)
            {
                Attribute a = Volatile.GetCustomAttribute(clrProperty, typeof(Volatile));
                if (clrProperty.Name != "EntityPOID" && a == null)
                {
                    IProperty prop = properties[clrProperty.Name];
                    fieldConverters.AddLast(new FieldConverter(t, clrProperty, prop, dataContext));
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
            IBusinessObject res = dataContext.IBOCache.Get(e.EntityPOID);
            if (res == null)
            {
                res = (IBusinessObject)instantiator();
                SetValues(e, res);
                dataContext.IBOCache.Add(res, e.EntityPOID );
            }
            return res;
        }

        /// <summary>
        /// Copies values from PropertyValues stored in 
        /// e to the fields in res. (Thus changing state 
        /// of res)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="res"></param>
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
            IEntity res = dataContext.GenDB.NewEntity();

            // Drop the db-created EntityPOID if DBTag is set.
            if (ibo.EntityPOID.IsInCache)
            {
                res.EntityPOID = ibo.EntityPOID;
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
}