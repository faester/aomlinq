using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

/* Om instantiering af generiske typer via reflection:
 * http://msdn2.microsoft.com/en-us/library/b8ytshk6.aspx
 */

namespace GenDB.DB
{
    //class OneElement<T>
    //{
    //    T theElement;

    //    public T TheElement
    //    {
    //        get { return theElement; }
    //        set { theElement = value; }
    //    }

    //}

    /// <summary>
    /// The list translator does not instantiate the collection it self. 
    /// This responsibility is left to the different collections.
    /// TODO: Everything ;)
    /// </summary>
    class BOListTranslator : IIBoToEntityTranslator
    {
        Type typeOfBOList = typeof(BOList<>);

        //public void HowTheFuck()
        //{
        //    //OneElement<t> e = new OneElement<t>();

        //    Type theType = typeof(int);
        //    object o = typeOfBOList.MakeGenericType(theType);
        //}

        public IBusinessObject Translate(IEntity ie)
        {
            IBusinessObject res = IBOCache.Get(ie.EntityPOID);
            if (res != null)
            {
                return res;
            }
            else
            {
                IPropertyType pt = ie.EntityType.GetProperty (TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME).PropertyType;
                return null;
            }
        }

        public IEntity Translate(IBusinessObject ibo)
        {
            throw new Exception("Not implemented.");
        }

        public void SetValues(IBusinessObject ibo, IEntity ie)
        {
            throw new Exception("Not implemented.");
        }

        public void SetValues(IEntity ie, IBusinessObject ibo)
        {
            throw new Exception("Not implemented.");
        }
    }
}
