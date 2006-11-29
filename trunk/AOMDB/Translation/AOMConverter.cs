using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using AOM;
using Persistence;

namespace Translation
{
    /// <summary>
    /// Handles translation between entities and objects. 
    /// The AOMConverter is not type safe, and should be 
    /// wrapped in an 
    /// </summary>
    internal class AOMConverter 
    {
        AOMConverter superConverter = null;
        LinkedList<FieldInfo> valFields;
        LinkedList<FieldInfo> refFields;
        Type objType;
        EntityType aomtype = null;

        private AOMConverter() { /* empty */ }

        private static void CheckTypeLegality(Type t)
        {
            if (t.IsGenericType || t.IsGenericTypeDefinition)
            {
                throw new NotTranslatableException("Can not translate generic types.");
            }
            Type ibusinessobjectInterface =
                t.GetInterface(typeof(Business.IBusinessObject).FullName);
            if (ibusinessobjectInterface == null)
            {
                throw new NotTranslatableException("Reference types must implement IBusinessObject (" + t.FullName + ")" );
            }
        }

        /// <summary>
        /// Creates a converter for the given object. 
        /// The field values etc are irrelevant, so 
        /// perhabs the obj.GetType() should simply be 
        /// passed ?
        /// </summary>
        /// <param name="obj"></param>
        public AOMConverter(object o, EntityType aomtype)
        {
            this.aomtype = aomtype;
            objType = o.GetType();
            Init();
        }

        public AOMConverter(Type ot, EntityType aomtype) {
            this.aomtype  = aomtype;
            this.objType = ot;
            Init();
        }

        private void Init()
        {
            refFields = new LinkedList<FieldInfo>();
            valFields = new LinkedList<FieldInfo>();
            FieldInfo[] allFields = objType.GetFields(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly
                );
            foreach (FieldInfo f in allFields)
            {
                if (f.FieldType.IsValueType || f.FieldType.Equals(typeof(string))) {
                    valFields.AddLast(f);
                }
                else
                {
                    if (f.FieldType.Equals(typeof(Persistence.DBTag)))
                    {
                        /* DBTag fields are silently ignored, 
                         * since they should be handled by 
                         * the database layer.
                         */
                    }
                    else
                    {
                        CheckTypeLegality(f.FieldType);
                        refFields.AddLast(f);
                    }
                }
            }
            if (aomtype.SuperType != null)
            {
                Type super = objType.BaseType;
                EntityType superEntityType = EntityTypeConverter.Construct(super);
                superConverter = new AOMConverter(super, superEntityType);
            }
        }

        /// <summary>
        /// Converts from an AOM entity to an object.
        /// This method assumes, that the object given
        /// is of the correct type, such that all in the 
        /// valFields entity corresponds to a field in the 
        /// object.
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="outObj"></param>
        /// <returns></returns>
        public object FromEntity(Entity e, object outObj) {
            if (e == null)
            {
                return null;
            }
            foreach(FieldInfo f in valFields) 
            {
                string s = e.GetPropertyValue(f.Name);
                f.SetValue(outObj, FieldConverter.ToFieldValue(f, s));
            }
            foreach (FieldInfo f in refFields)
            {
                string s = e.GetPropertyValue(f.Name);
                f.SetValue(outObj, FieldConverter.ToObject(s));
            }
            if (superConverter != null) {
                outObj = superConverter.FromEntity(e.EntityBase, outObj);
            }
            if (e.Id != Entity.UNDEFINED_ID)
            {
                BOCache.Store(outObj, e.Id);
            }
            return outObj;
        }

        /// <summary>
        /// Sets the fields of the Entity given in 
        /// accordance with the field value of the 
        /// object given. Returns the parameter e1.
        /// <para>PRE: e1 must be of same type as obj 
        /// or a subtype of obj.</para>
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="obj"></param>
        /// <returns>parameter e1</returns>
        public Entity ToEntity(Entity e, object obj) {
            e.IsPersistent = false;
            foreach (FieldInfo f in valFields)
            {
                object o  = f.GetValue(obj);
                if (o != null)
                {
                    e.SetProperty(f.Name, FieldConverter.ToValueString(f, o));
                }
                else
                {
                    e.SetProperty(f.Name, null);
                }
            }
            foreach (FieldInfo f in refFields) 
            {
                object o  = f.GetValue(obj);
                if (o != null)
                {
                    e.SetProperty(f.Name, FieldConverter.ToEntityPOIDString(o));
                }
                else
                {
                    e.SetProperty(f.Name, null);
                }
            }
            if (superConverter != null) {
               superConverter.ToEntity(e.EntityBase, obj);
            }
            return e;
        }

    }
}
