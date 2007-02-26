using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Volatile : Attribute { }

    /// <summary>
    /// Indicates that the user
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LazyLoad : Attribute
    {
        public string Storage;
        private LazyLoad() {}
        public LazyLoad(string storage) { 
            Storage = storage;
        }
    }

    public class LazyLoader
    {
        private bool isLoaded = false;
        private bool isNullReference = false;

        internal bool IsNullReference
        {
            get { return isNullReference; }
            set { isNullReference = value; }
        }

        internal bool IsLoaded
        {
            get { return isLoaded; }
            set { isLoaded = value; }
        }

        internal int entityPOID = 0;

        protected internal LazyLoader() 
        {
            isNullReference = true;
            isLoaded = true;
        }

        /// <summary>
        /// For non-null references, use this
        /// instantiator.
        /// </summary>
        /// <param name="entityPOID"></param>
        internal LazyLoader(int entityPOID)
        {
            this.entityPOID = entityPOID;
            isLoaded = false;
            isNullReference = false;
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
            this.isNullReference = isNullReference;
        }

        internal IBusinessObject LoadObject()
        {
            IBusinessObject ibo = DataContext.Instance.GenDB.GetByEntityPOID(entityPOID);
            isNullReference = (ibo == null);
            return ibo;
        }
    }

    public sealed class LazyLoader<T> : LazyLoader
        where T : IBusinessObject
    {
        /// <summary>
        /// When using this constructor the element 
        /// will be set to default(t);
        /// </summary>
        public LazyLoader() { 
            IsLoaded = true; 
            IsNullReference = true;
            element = default(T);
        }
        private T element;

        public LazyLoader(T element)
        {
            this.element = element;
            IsLoaded = true;
        }

        public T Element
        {
            get {
                if (!IsLoaded)
                {
                    IsLoaded = true;
                    Load();
                }
                return element;
            }
            set {
                IsLoaded = true;
                IsNullReference = (value == null);
                element = value;
            }
        }

        private void Load()
        {
            element = (T)LoadObject();
        }
    }

}
