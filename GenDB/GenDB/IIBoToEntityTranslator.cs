using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB
{
    internal interface IIBoToEntityTranslator
    {
        IEntityType EntityType { get; }

        IBusinessObject CreateInstanceOfIBusinessObject();

        IEnumerable<FieldConverter> FieldConverters { get; }

        void SetProperty(long propertyPOID, IBusinessObject obj, object propertyValue);

        IEntity Translate (IBusinessObject ibo);

        void SetValues(IBusinessObject ibo, IEntity ie);    

        /// <summary>
        /// This method is introduced to allow different save 
        /// algorithms for regular objects and collections.
        /// It is - however - rather unelegant, since the calling
        /// object (IBOCache) should ensure, that the translator
        /// actually matches the IBusinessObject given.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="res"></param>
        void SaveToDB(IGenericDatabase db, IBusinessObject ibo);
    }
}
