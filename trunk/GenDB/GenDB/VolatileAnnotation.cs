using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    public class Volatile : Attribute { }

    /// <summary>
    /// Indicates that the user
    /// </summary>
    public class LazyLoad : Attribute
    {
        public string Storage;
    }

    public class LazyLoader
    {
        protected bool isLoaded = false;

        internal int entityPOID = 0;

        protected internal LazyLoader() {}

        /// <summary>
        /// For non-null references, use this
        /// instantiator.
        /// </summary>
        /// <param name="entityPOID"></param>
        internal LazyLoader(int entityPOID)
        {
            this.entityPOID = entityPOID;
            isLoaded = false;
        }

        /// <summary>
        /// If the database indicates this should be volatileAttribute null 
        /// reference, instantiate using this constructor
        /// and pass true as parameter.
        /// </summary>
        /// <param name="isNullReference"></param>
        internal LazyLoader(bool isNullReference)
        {
            isLoaded = isNullReference;
        }

        internal IBusinessObject LoadObject()
        {
            return DataContext.Instance.GenDB.GetByEntityPOID(entityPOID);
        }
    }

    public sealed class LazyLoader<T> : LazyLoader
        where T : IBusinessObject
    {
        public LazyLoader() { isLoaded = true; }
        private T element;

        public LazyLoader(T element)
        {
            this.element = element;
            isLoaded = true;
        }

        public T Element
        {
            get {
                if (!isLoaded)
                {
                    isLoaded = true;
                    Load();
                }
                return element;
            }
            set {
                isLoaded = true;
                element = value;
            }
        }

        private void Load()
        {
            element = (T)LoadObject();
        }
    }
}
