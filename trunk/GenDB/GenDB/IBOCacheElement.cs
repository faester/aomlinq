using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    /// <summary>
    /// Use to store committed objects in the cache.
    /// Will store a clone of the object given at instantiation
    /// time to enable change tracking. 
    /// </summary>
    internal sealed class IBOCacheElement : IDisposable
    {
        WeakReference wr; // Used temporarely when cache element is trying to allow garbage collection of its object
        IBusinessObject clone; // The object in its state when cache element was instantiated
        IBusinessObject element; // The element with ordinary reference. Reflects the application state of the object.

        int entityPOID;
        bool isCollection;

        public int EntityPOID
        {
            get { return entityPOID; }
        }

        private IBOCacheElement() { /* empty */ }

        public IBOCacheElement(IBusinessObject target)
        {
            if (target.GetType().GetInterface("IDBSaveableCollection") != null) 
            { 
                isCollection = true; 
            }
            element = target;

            wr = new WeakReference(element);
            
            entityPOID = target.DBIdentity;
            
            SetNotDirty();
        }

        /// <summary>
        /// Contains the element given at instantiation time.
        /// </summary>
        public IBusinessObject Element
        {
            get { return element; }
        }


        /// <summary>
        /// Will temporarely store the element in a WeakReference 
        /// to allow the element to be garbage collected. 
        /// 
        /// The class will not function as expected until the
        /// ReEstablishStrongReference() method has been called.
        /// </summary>
        public void ReleaseStrongReference()
        {
            element = null;
        }


        /// <summary>
        /// Will swap the internal storage of the element 
        /// from a weak reference to a regular reference.
        /// </summary>
        public void ReEstablishStrongReference()
        {
            element = (wr.Target as IBusinessObject);
        }

        /// <summary>
        /// Tests if the element stored has been reclaimed by the garbage
        /// collector. Can only be performed between a call to ReleaseStrongReference and
        /// ReEstablishStrongReference.
        /// </summary>
        public bool IsAlive
        {
            get { return wr.IsAlive; }
        }


        /// <summary>
        /// Returns true if the element has changes state since this 
        /// cache element was instantiated or since last call to SetNotDirty
        /// </summary>
        public bool IsDirty
        {
            get { 
                IIBoToEntityTranslator translator = DataContext.Instance.Translators.GetTranslator(element.GetType());
                return !translator.CompareProperties(element, clone);
            }
        }

        private IIBoToEntityTranslator GetElementsTranslator()
        {
            return DataContext.Instance.Translators.GetTranslator(element.GetType());
        }

        public void SetNotDirty()
        {
            //IIBoToEntityTranslator translator = DataContext.Instance.Translators.GetTranslator(element.GetType());
            clone = GetElementsTranslator().CloneObject(element);
            //if (!isCollection)
            //{
            //    clone = (IBusinessObject)ObjectUtilities.MakeClone((element as IBusinessObject));
            //}
            //else
            //{
            //    (element as IDBSaveableCollection).HasBeenModified = false;
            //}
        }

        #region IDisposable Members

        public void Dispose()
        {
            wr = null;
            element = null;
            clone = null;
        }

        #endregion
    }
}
