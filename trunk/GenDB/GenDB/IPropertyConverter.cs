using System;
namespace GenDB
{
    interface IPropertyConverter
    {
        long PropertyPOID { get; set; }
        Type PropertyType { get; }
        bool ReferenceCompare { get; set; }
        void SetEntityPropertyValue(IBusinessObject ibo, GenDB.DB.IEntity e);
        PropertySetter PropertySetter { get; }

        /// <summary>
        /// Copies the field described by this IPropertyConverter 
        /// from source to target.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        void CloneProperty(object source, object target);
    }
}
