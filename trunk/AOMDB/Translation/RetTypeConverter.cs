using System;
using System.Collections.Generic;
using Persistence;
using AOM;
using System.Reflection;

namespace Translation
{
    /// <summary>
    /// Converts fields with reference types to their objects
    /// and vice versa. The EntityPOID is stored as a string in
    /// the Entity object. 
    /// <para>
    /// The conversion of reference type fields is made complicated 
    /// by the fact that object and class types may differ, why it 
    /// is not immidiately possibly to store the converters for a field.
    /// If the object type is a sub-type of the class type, a more 
    /// specialized converter will be needed. This makes it difficult 
    /// to store fixed converters for a class-entity converter.
    /// </para>
    /// </summary>
    class RefTypeConverter : IFieldConverter
    {
        //private static Dictionary<long, object> idObj = null;
        //private static Dictionary<object, long> objId = null;

        private static RefTypeConverter instance = new RefTypeConverter();

        public static RefTypeConverter Instance { get { return instance; } }

        static RefTypeConverter()
        {
            //idObj = new Dictionary<long, object>();
            //objId = new Dictionary<object, long>();
        }

        private RefTypeConverter() { /* empty */ }

        public object ToPropertyValue(string propertyValue)
        {
            if (propertyValue == null)
            {
                return null;
            }
            long entityPOID = long.Parse(propertyValue);
            if (BOCache.HasId(entityPOID))
            {
                object res = BOCache.GetObjectByID(entityPOID);
                return res;
            }

            Entity e = BOCache.RetrieveEntity(entityPOID);

            Type t = Type.GetType(e.Type.Name);

            ConstructorInfo cinf = t.GetConstructor(null);
            object o = cinf.Invoke(null);

            AOMConverter aomcnv = new AOMConverter(t, e.Type);
            aomcnv.FromEntity(e, o);

            BOCache.Store(o, entityPOID);

            return o;
        }

        /// <summary>
        /// Gets a value string for the object. (This is the entityPOID as string.)
        /// <p>
        /// TODO: For the moment the Entity representation 
        /// is stored twice, first time is simply to get a
        /// global unique ID for the object. 
        /// Instead the DB should make it possible 
        /// to reserve ID numbers. We must work on this later.
        /// TODO: (number 2) Should store the EntityType for a given type, when 
        /// constructed as should the AOMConverter.
        /// </p>
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public string ToValueString(object o)
        {
            if (o == null) { return null; }            
            if (BOCache.HasObject(o))
            {
                return BOCache.GetIDByObject(o).ToString();

            }
            else
            {
                //EntityType et = EntityTypeConverter.Construct(o);
                //AOMConverter cnv = new AOMConverter(o, et);
                //Entity e = et.New();
                //Set id. 
                //e.Id = BOCache.GetNewUnusedId ();
                //BOCache.StoreEntity(e);
                long entityPOID = BOCache.GetNewUnusedId();

                BOCache.Store(o, entityPOID);
                //cnv.ToEntity(e, o);
                //BOCache.StoreEntity(e);
                return entityPOID.ToString();
            }
        }

        /// <summary>
        /// TODO: Principielt burde dette kunne løses ved 
        /// at teste om EntityPOID er identiske, men indtil 
        /// videre antages "false".
        /// </summary>
        public bool CanHandleEquals
        {
            get { return true; }
        }
    }
}
