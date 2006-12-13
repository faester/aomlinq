using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Query;
using System.Expressions;

namespace GenDB
{
    // IQueryable<T>,

    public class Table<T> : ICollection<T>, IEnumerable<T>
        where T : IBusinessObject
    {
        IWhereable expression;

        #region ICollection members

        public void Add(T e)
        {
            throw new Exception("Not implemented");
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
            IEntityType lastType = null;
            DelegateTranslator translator = null;
            foreach (IEntity e in Configuration.GenDB.Where(expression))
            {
                if (lastType != e.EntityType)
                {
                    translator = TypeSystem.GetTranslator (e.EntityType.EntityTypePOID);
                }
                IBusinessObject ibo = translator.Translate (e);
                yield return (T)ibo;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Console.WriteLine("SECOND");
            return GetEnumerator();			  
        }



        #endregion

        public Table() { /* empty */ }

        #region Query Expression 

        public Table<T> Where(Expression<Func<T, bool>> expr)
        {
            MSSqlExprTranslator exprTranslator = new MSSqlExprTranslator();
            this.expression = exprTranslator.Convert (expr);
            return this;
        }

        public ICollection<T> Select<U>(Expression<Func<T, U>> projection)
        {
            throw new Exception("Select ikke implementeret.");
        }

        #endregion
    }
}
