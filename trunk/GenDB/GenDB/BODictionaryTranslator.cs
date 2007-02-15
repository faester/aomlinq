using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GenDB.DB;

namespace GenDB
{
    class BODictionaryTranslator : BaseTranslator
    {
        public static readonly Type TypeOfBODictionary = typeof(BODictionary<,>);
        public static readonly string MAPPING_PROPERTY_NAME = "Mapping";

        public BODictionaryTranslator(Type t, DataContext dataContext, IEntityType bodictEntityType)
            : base(t, bodictEntityType, dataContext)
        {        }

        /// <summary>
        /// Stores the PropertyInfo array of fields to translate.
        /// </summary>
        protected override PropertyInfo[] GetPropertiesToTranslate()
        {
            return new PropertyInfo[0]; // Discard properties
        }

        public override void SaveToDB(IBusinessObject ibo)
        {
            //throw new Exception("not implemented");
            Type t = ibo.GetType();
            if (!t.IsGenericType || t.GetGenericTypeDefinition() != TypeOfBODictionary )
            {
                throw new NotTranslatableException("Internal error: BOListTranslator can not translate Type ", ibo.GetType());
            }
            IEntity e = Translate(ibo);
            dataContext.GenDB.Save (e);

            (ibo as IDBSaveableCollection).SaveElementsToDB();
        }

        public override void SetProperty(long propertyPOID, IBusinessObject obj, object propertyValue)
        {
        }
    }
}
