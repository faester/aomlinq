using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Query;
using System.Expressions;
using GenDB.DB;

namespace GenDB
{
    // IQueryable<T>,

    public class Table<T> : ICollection<T>, IEnumerable<T>
        where T : IBusinessObject
    {   
        IWhereable expression = new ExprInstanceOf(typeof(T));

        #region ICollection members

        public void Add(T ibo)
        {
            if (!TypeSystem.IsTypeKnown(ibo.GetType()))
            {
               TypeSystem.RegisterType(ibo.GetType());
            }
            IIBoToEntityTranslator trans = TypeSystem.GetTranslator(ibo.GetType());
            IEntity e = trans.Translate(ibo);
            Configuration.GenDB.Save(e);
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
            get{return Configuration.GenDB.Where(expression).Count();}
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            IEntityType lastType = null;
            IIBoToEntityTranslator translator = null;
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
            return GetEnumerator();			  
        }

        #endregion

        public Table() { /* empty */ }

        #region Query Expression 

        public Table<T> Where(Expression<Func<T, bool>> expr)
        {
            SqlExprTranslator exprTranslator = new SqlExprTranslator();
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
