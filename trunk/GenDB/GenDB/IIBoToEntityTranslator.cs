using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB
{
    internal interface IIBoToEntityTranslator
    {
        /// <summary>
        /// The IEntityType describing the class 
        /// translatable by this translator.
        /// </summary>
        IEntityType EntityType { get; }

        /// <summary>
        /// Instantiates a new instance of the class 
        /// associated with this IIBoToEntityTranslator
        /// </summary>
        /// <returns></returns>
        IBusinessObject CreateInstanceOfIBusinessObject();

        /// <summary>
        /// The propertyconverters associted with this 
        /// translator and and all super types translators.
        /// </summary>
        IEnumerable<IPropertyConverter> PropertyConverters { get; }

        /// <summary>
        /// Sets the property of obj described by IProperty with 
        /// propertyPOID to the value of propertyValue.
        /// </summary>
        /// <param name="propertyPOID">ID of property to set</param>
        /// <param name="obj">The IBusinessObject that should have its property set</param>
        /// <param name="propertyValue">value for the property. Must be castable to the type of the Property</param>
        void SetProperty(long propertyPOID, IBusinessObject obj, object propertyValue);

        /// <summary>
        /// Sets IPropertyValues of the given IEntity object in 
        /// accordance with 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ie"></param>
        void SetValues(IBusinessObject ibo, IEntity ie);    

        /// <summary>
        /// Gets the IPropertyConverter for the IProperty with 
        /// id = propertyPOID.
        /// </summary>
        /// <param name="propertyPOID">ID for the desired propert</param>
        /// <returns>IPropertyConverter for the property</returns>
        IPropertyConverter GetPropertyConverter(int propertyPOID);

        /// <summary>
        /// Gets the IPropertyConverter for an IProperty.
        /// The property must exist for the type associated 
        /// with this translator or a subtypes translator.
        /// </summary>
        /// <param name="propertyPOID">ID for the desired propert</param>
        /// <returns>IPropertyConverter for the property</returns>
        IPropertyConverter GetPropertyConverter(IProperty property);

        /// <summary>
        /// This method is introduced to allow different save 
        /// algorithms for regular objects and collections.
        /// It is - however - rather unelegant, since the calling
        /// object (IBOCache) should ensure, that the translator
        /// actually matches the IBusinessObject given.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="clone"></param>
        void SaveToDB(IBusinessObject ibo);
    }
}
