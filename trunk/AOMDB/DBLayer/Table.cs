using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Expressions;
using Business;
using Translation;
using Persistence;
using AOM;

namespace DBLayer
{
    public class Table<T> : /* IQueryable<T>, */ ICollection<T>
        where T : IBusinessObject
    {
        BO2AOMTranslator <T> converter = new BO2AOMTranslator<T>();
        int capacity = 1; //TODO: Remove
        int size = 0; //TODO: Remove
        T[] content; //TODO: Remove

        #region ICollection members

        public void Add(T e)
        {
            Entity e = converter.ToEntity (e);
            Database.Instance.Store (e);
        }

        public void Clear()
        {
            throw new Exception("Not implemented");
        }

        public bool Contains(T e)
        {
            throw new Exception("Not implemented");
        }

        public void CopyTo(T[] arr, int i)
        {
            throw new Exception("Not implemented");
        }

        public bool Remove(T e)
        {
            if (e.DatabaseID != null)
            {
                DBTag tag = e.DatabaseID ;
                ObjectCache.RemoveByID (tag.Id);
                Database.Instance.Delete (tag);
            }
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
        #endregion

        #region Table members
        public Table()
        {
            throw new Exception("Not implemented");
        }

        private void Resize(int newCapacity)
        {
            throw new Exception("Not implemented");
        }

        public IQueryable CreateQuery(Expression e)
        {
            throw new Exception("Not implemented");
        }

        public object Execute(Expression e)
        {
            return e;
        }

        public Expression Expression
        {
            get
            {
                throw new Exception("Not implemented");
            }
        }

        public Type ElementType
        {
            get { return typeof(T); }
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

        // Polymorfien kan ikke operere med denne og ovenstående samtidig
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
