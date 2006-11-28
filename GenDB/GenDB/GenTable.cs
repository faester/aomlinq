using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    /// <summary>
    /// The table abstraction visible to the application 
    /// layer. Handles regular objects, conversion to generic
    /// types is handled "internally" (substitute).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenTable<T> : ICollection<T>
            where T : new()
    {
        public void CopyTo(T[] arr, int index)
        {
            throw new Exception("Not implemented");
        }

        public void Add(T element)
        {
            throw new Exception("Not implemented");
        }

        public void Clear()
        {
            throw new Exception("Not implemented");
        }

        public bool Contains(T key)
        {
            throw new Exception("Not implemented");
        }

        public bool Remove(T key)
        {
            throw new Exception("Not implemented");
        }

        public int Count
        {
            get { throw new Exception("Not implemented"); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            throw new Exception("Not implemented");
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
