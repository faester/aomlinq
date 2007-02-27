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


    /// <summary>
    /// Sets a specific IProperty of the given 
    /// IEntity object to value given.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="value"></param>
    delegate void PropertyValueSetter(IEntity e, object value);

    /// <summary>
    /// Sets a property value of the given IBusinessObject to
    /// the value contained in value.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="value"></param>
    delegate void PropertySetter(IBusinessObject ibo, object value);


    /// <summary>
    /// Translates between IBusinessObject and IEntity. Not type safe, so the 
    /// IBOTranslator should be stored in a hash table with types as key.
    /// (Or be instantiated anew for each type, which is of course less effective
    /// due to instantiation time.)
    /// </summary>
    class IBOTranslator : BaseTranslator
    {
        public IBOTranslator(Type t, IEntityType iet, DataContext dataContext)
            : base(t, iet, dataContext)
        { }

        public override void SaveToDB(IBusinessObject ibo)
        {
            IEntity e = this.Translate(ibo);
            this.dataContext.GenDB.Save(e);
        }

        /// <summary>
        /// Stores the PropertyInfo array of fields to translate.
        /// </summary>
        protected override PropertyInfo[] GetPropertiesToTranslate()
        {
            return t.GetProperties(
                BindingFlags.Public
                | BindingFlags.DeclaredOnly
                | BindingFlags.Instance
                );
        }

        public override void SetProperty(long propertyPOID, IBusinessObject obj, object propertyValue)
        {
            fieldConverterDict[propertyPOID].PropertySetter(obj, propertyValue);
        }
    }

    public class UnknownPropertyException : Exception
    {
        int propertyPOID;
        IProperty property;

        internal IProperty Property
        {
            get { return property; }
        }

        public int PropertyPOID
        {
            get { return propertyPOID; }
        }

        internal UnknownPropertyException(int propertyPOID)
        {
            this.propertyPOID = propertyPOID;
        }

        internal UnknownPropertyException(IProperty property)
        {
            this.propertyPOID = property.PropertyPOID;
            this.property = property;
        }
    }


}