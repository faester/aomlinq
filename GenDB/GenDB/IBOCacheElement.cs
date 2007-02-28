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

        int entityPOID; // The entityPOID is stored seperately, since the property might be accessed, when the element has been set to null to allow garbage collection.

        public int EntityPOID
        {
            get { return entityPOID; }
        }

        private IBOCacheElement() { /* empty */ }

        /// <summary>
        /// Target must not be null. (This is ensured both in the Table 
        /// and IBOCache, so no check is done, but heaven breaks loose
        /// if this precondition is violated.)
        /// </summary>
        /// <param name="target"></param>
        public IBOCacheElement(IBusinessObject target)
        {
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
                return !GetElementsTranslator().CompareProperties(element, clone);
            }
        }

        /// <summary>
        /// Returns the IIBoToEntityTranslator associated with 
        /// the stored element. 
        /// </summary>
        /// <returns></returns>
        private IIBoToEntityTranslator GetElementsTranslator()
        {
            return DataContext.Instance.Translators.GetTranslator(element.GetType());
        }

        /// <summary>
        /// Registers that the stored element is no longer dirty. 
        /// (Creates a new clone)
        /// </summary>
        public void SetNotDirty()
        {
            clone = GetElementsTranslator().CloneObject(element);
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
