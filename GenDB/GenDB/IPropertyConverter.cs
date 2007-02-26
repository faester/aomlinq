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
        /// Source and target must be instanceof the type holding the property 
        /// described by the converter and must be of identical type.
        /// Neither source nor target can be null.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        void CloneProperty(object source, object target);


        /// <summary>
        /// Compares the field described by this IPropertyConverter 
        /// in a and b.
        /// Source and target must be instanceof the type holding the property 
        /// described by the converter and must be of identical type.
        /// Neither a or b must be null.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        bool CompareProperties(object a, object b);
    }
}
