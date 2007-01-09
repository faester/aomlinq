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

        /// <summary>
        /// Will clear all elements from database, that matches this tables current
        /// where expression. (For regularly instantiated objects this means all 
        /// entities of type T or a subclass of this type.)
        /// </summary>
        public void Clear()
        {
            Configuration.GenDB.WhereClear(expression);
        }

        public bool Contains(T e)
        {
            throw new Exception("Not implemented");
        }

        public void CopyTo(T[] arr, int i)
        {
            int idx = i;

            foreach(T t in this)
            {
                arr[idx++] = t;
            }
        }

        public bool Remove(T e)
        {
            throw new Exception("Not implemented");
        }

        public int Count
        {
            get { return Configuration.GenDB.Count(expression); }
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
        
        #endregion
    }
}
