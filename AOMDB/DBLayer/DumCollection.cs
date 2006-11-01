using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Expressions;

namespace DBLayer
{
    public class DumCollection<T> : /* IQueryable<T>, */ ICollection<T>
    {
        int capacity = 1;
        int size = 0;
        T[] content;

        #region ICollection members

        public void Add(T e)
        {
            if (size == capacity)
            {
                Resize(capacity * 2);
            }
            content[size++] = e;
        }

        public void Clear()
        {
            capacity = 1;
            size = 0;
            content = new T[capacity];
        }

        public bool Contains(T e)
        {
            for (int idx = 0; idx < size; idx++)
            {
                if (content[idx].Equals(e)) { return true; }
            }
            return false;
        }

        public void CopyTo(T[] arr, int i)
        {
            for (int idx = 0; idx < size; idx++)
            {
                arr[idx] = content[idx + i];
            }
        }

        public bool Remove(T e)
        {
            bool res = false;
            for (int idx = 0; idx < size; idx++)
            {
                if (content[idx].Equals(e))
                {
                    res = true;
                    while (idx < size && content[idx].Equals(e))
                    {
                        content[idx] = content[(size--) - 1];
                    }
                }
            }
            return res;
        }

        public int Count
        {
            get { return size; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            for (int idx = 0; idx < size; idx++)
            {
                yield return content[idx];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region DumCollection members
        public DumCollection()
        {
            content = new T[capacity];
        }

        private void Resize(int newCapacity)
        {
            if (newCapacity == 0) { newCapacity = 1; }
            Array.Resize<T>(ref content, newCapacity);
            capacity = newCapacity;
        }

        public IQueryable CreateQuery(Expression e)
        {
            return null;
        }

        public object Execute(Expression e)
        {
            return e;
        }

        public Expression Expression
        {
            get
            {
                return null;
            }

        }

        public Type ElementType
        {
            get { return null; }
        }
        #endregion

        #region Query Expression Pattern members

        public DumCollection<T> Where (Expression<Func<T, bool>> expr)
        {
            Console.WriteLine("Where kaldt med Exression {0}: ", expr.ToString ());
            Func<T, bool> predicate = expr.Compile();
            DumCollection<T> result = new DumCollection<T>();
            for (int idx = 0; idx < size; idx++)
            {
                if (predicate (content[idx]))
                {
                    result.Add(content[idx]);
                }
            }
            return result;
            //return this.Where<T> (expr.Compile()).ToQueryable();
        }

        // Kan ikke operere med denne og ovenstående samtidig
        //public IEnumerable<T> Where(Func<T, bool> func)
        //{
        //    DumCollection<T> where = new DumCollection<T>();
        //    foreach (T t in content)
        //    {
        //        if (func(t))
        //        {
        //            where.Add(t);
        //        }
        //    }
        //    return where;
        //}

        
        public IEnumerable<S> Select<S>(Expression<Func<T, S>> selector)
        {
            Console.WriteLine ("Select called with expression: {0}", selector);
            Func<T, S> sel = selector.Compile();
            DumCollection<S> result = new DumCollection<S>();
            for (int i = 0; i < size; i++)
            {
                result.Add  (sel( content[i]));
            }
            return result;
        }

        public IEnumerable<S> SelectMany<S>(Func<T, IEnumerable<S>> sel)
        {
            Console.WriteLine("SelectMany got selector : {0}", sel);
            return new DumCollection<S>();
        }
        #endregion
    }
}
