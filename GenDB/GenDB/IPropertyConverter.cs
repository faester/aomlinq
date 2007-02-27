using System;
namespace GenDB
{
    interface IPropertyConverter
    {

        /// <summary>
        /// The id of the property this IPropertyConverter can convert
        /// </summary>
        long PropertyPOID { get; set; }

        /// <summary>
        /// The Type object describing the type of the Property 
        /// translatable by this IPropertyConverter
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// Indicates if the associated property should be
        /// compared using reference comparison.
        /// </summary>
        bool ReferenceCompare { get; set; }

        /// <summary>
        /// Will set the property associated with this IPropertyConverter
        /// of the given IEntity to the value held by the property in source.
        /// </summary>
        /// <param name="source">Where to get property</param>
        /// <param name="taret">Will set IPropertyValue in this IEntity</param>
        void SetEntityPropertyValue(IBusinessObject source, GenDB.DB.IEntity target);


        /// <summary>
        /// The propertysetter associated with this object
        /// </summary>
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
