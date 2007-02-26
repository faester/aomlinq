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

}
