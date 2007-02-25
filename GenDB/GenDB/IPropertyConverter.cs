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
    }
}
