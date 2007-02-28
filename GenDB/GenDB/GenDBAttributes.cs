using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    /// <summary>
    /// Indicates that the user does not want this property to 
    /// be persisted, even though it is public and has both a 
    /// getter and a setter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Volatile : Attribute { }

    /// <summary>
    /// Indicates that the user wants the property to be 
    /// retrieved lazyly. The constructor takes a string 
    /// naming the field LazyLoader<typeparamref name="T"/>
    /// that should hold the properties value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LazyLoad : Attribute
    {
        public string Storage;
        public LazyLoad() {}
        //public LazyLoad(string storage) { 
        //    Storage = storage;
        //}
    }

}
