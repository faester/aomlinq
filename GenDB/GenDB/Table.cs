using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using GenDB.DB;

namespace GenDB
{
    // IQueryable<T>,

    public class Table<T> :  ICollection<T>, IEnumerable<T>
        where T : IBusinessObject
    {
        IExpression expression = new ExprInstanceOf(typeof(T));
        IGenericDatabase db = null;
        TranslatorSet translators = null;
        TypeSystem typeSystem;
        IBOCache iboCache;
        Func<T, bool> linqFunc = null;
        bool exprFullySqlTranslatable = true;

        public bool ExprFullySqlTranslatable
        {
            get { return exprFullySqlTranslatable; }
        }


        #region Constructors
        private Table() { /* empty */ }

        internal Table(IGenericDatabase db, TranslatorSet translators, TypeSystem typeSystem, IBOCache iboCache)
        {
            if (db == null) { throw new NullReferenceException ("db"); }
            if (translators == null) { throw new NullReferenceException("translators"); }
            if (typeSystem == null) { throw new NullReferenceException("typeSystem"); }
            if (iboCache == null) { throw new NullReferenceException("iboCache"); }

            this.db = db;
            this.translators = translators;
            this.typeSystem = typeSystem;
            this.iboCache = iboCache;
            
            if (!typeSystem.IsTypeKnown(typeof(T)))
            {
                typeSystem.RegisterType(typeof(T));
            }
        }
        #endregion

        internal IWhereable Expression
        {
            get { return expression; }
        }

        #region ICollection members

        public void Add(T ibo)
        {
            if (ibo == null) { throw new NullReferenceException("Value can not be null."); }
            Type t = ibo.GetType();
            if (!typeSystem.IsTypeKnown(t))
            {
               typeSystem.RegisterType(t);
            }
            IIBoToEntityTranslator trans = translators.GetTranslator(t);
            trans.SaveToDB(db, ibo);
        }

        /// <summary>
        /// Will clear all elements from database, that matches this tables current
        /// where expression. (For regularly instantiated objects this means all 
        /// entities of type T or a subclass of this type.)
        /// <br>
        /// Elements are also removed from the cache to ensure that they will not 
        /// be accidentally added later on. This slows down the operation, since 
        /// it is neccessary run the where-query twice. 
        /// </br>  
        /// </summary>
        public void Clear()
        {
            if (exprFullySqlTranslatable)
            {
                foreach (T ie in db.Where(expression))
                {
                    iboCache.Remove(ie.DBIdentity);
                }
                db.ClearWhere(expression);
            }
            else
            {
                foreach (T deleteCandidate in db.Where(expression))
                {
                    if (linqFunc(deleteCandidate))
                    {
                        iboCache.Remove(deleteCandidate.DBIdentity);
                        db.ClearWhere(new BoolEquals (new VarReference(deleteCandidate), CstThis.Instance));
                    }
                }
            }
        }


        /// <summary>
        /// Returns true, if the element given exists in the database.
        /// Equality is resolved solely on the DBIdentity of the objects.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool Contains(T e)
        {
            if (e == null) { return false;}
            if (e.DBIdentity == 0) { return false; }
            IExpression where = new BoolEquals(new VarReference(e), CstThis.Instance);
            foreach (T ibo in db.Where(where))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Copies all persisted elements contained 
        /// in the table to the array given starting 
        /// from at position index in the array.
        /// </summary>
        /// <param name="arr">Target array for elements</param>
        /// <param name="index">Starting position in array</param>
        public void CopyTo(T[] arr, int index)
        {
            foreach(T t in this)
            {
                if(exprFullySqlTranslatable || linqFunc(t))
                {
                    arr[index++] = t;
                }
            }
        }

        /// <summary>
        /// Removes element 'e' from database if it exists. 
        /// Returns true, if element exists, false otherwise. 
        /// </summary>
        /// <param name="e">Element to remove.</param>
        /// <returns></returns>
        public bool Remove(T e)
        {
            if (e == null) { return false;}
            if (e.DBIdentity == 0) { return false; }
            IWhereable where = new BoolEquals(new VarReference(e), CstThis.Instance);
            return db.ClearWhere(where);
        }

        public int Count
        {
            get { 
                if (exprFullySqlTranslatable)
                {
                    return db.Count(expression); 
                }
                else
                {
                    int count = 0;
                    foreach (T ibo in db.Where(expression))
                    {
                        if (linqFunc(ibo)) { count++; }
                    }
                    return count;
                }
            }
        }

        public int CountEverything()
        {
            IWhereable where = new ExprAnd(new ExprInstanceOf(typeof(AbstractBusinessObject)), ExprIsTrue.Instance);
            return db.Count(where);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            foreach (T e in db.Where(expression))
            {
                if (exprFullySqlTranslatable)
                {
                    yield return e;
                }
                else
                {
                    if (linqFunc(e))
                    {
                        yield return e;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();			  
        }

        #endregion

        #region Query Expression 

        public Table<T> Where(Expression<Func<T, bool>> expr)
        {
            Table<T> res = new Table<T>();
            
            res.translators = this.translators ;
            res.typeSystem = this.typeSystem ;
            res.db = this.db;
            res.iboCache = this.iboCache;
            
            SqlExprTranslator exprTranslator = new SqlExprTranslator(typeSystem);
            SqlExprChecker checker = new SqlExprChecker();
            IExpression sqlExpr = new ExprAnd( exprTranslator.Convert (expr), this.expression);
            checker.StartVisit(sqlExpr);
            res.expression = sqlExpr;
            
            res.exprFullySqlTranslatable = (!checker.HasModifiedExpression) && this.exprFullySqlTranslatable;
            if (!res.exprFullySqlTranslatable)
            {
                if (!this.exprFullySqlTranslatable)
                {
                    Func<T, bool> f = expr.Compile();
                    Func<T, bool> andedFunc = delegate(T element) { return linqFunc(element) && f(element); };
                    res.linqFunc = andedFunc;
                }
                else
                {
                    res.linqFunc = expr.Compile();
                }
            }
            else
            {
                res.linqFunc = null;
            }

            return res;
        }
        // inner join
        //public IEnumerable<V> Join<U, K, V>(IEnumerable<U> inner, 
        //                              Expression<Func<T, K>> outerKeySelector,
        //                              Expression<Func<U, K>> innerKeySelector,
        //                              Expression<Func<T, U, V>> resultSelector)
        //{
        //    Table<T> res = new Table<T>();
        //    if (TranslatorChecks.ImplementsIBusinessObject(typeof(U)) && TranslatorChecks.ImplementsIBusinessObject(typeof(T)))
        //    {
        //        SqlJoinTranslator joinTranslator = new SqlJoinTranslator(typeSystem);
        //        IExpression exe = joinTranslator.Convert(outerKeySelector,innerKeySelector,false);
        //        throw new Exception("do translation");
        //    }
        //    else
        //    {
        //        return Queryable.Join<T, U, K, V>(this.ToQueryable(), inner.ToQueryable(), outerKeySelector, innerKeySelector, resultSelector);
        //    }
        //}

        // outer join
        //public IEnumerable<V> GroupJoin<U, K, V>(IEnumerable<U> inner, 
        //                                        Expression<Func<T, K>> outerKeySelector,
        //                                        Expression<Func<U, K>> innerKeySelector,
        //                                        Expression<Func<T, IEnumerable<U>, V>> resultSelector)
        //{
        //    Table<T> res = new Table<T>();
        //    if(TranslatorChecks.ImplementsIBusinessObject(typeof(U)) && TranslatorChecks.ImplementsIBusinessObject(typeof(T)))
        //    {
        //        SqlJoinTranslator joinTranslator = new SqlJoinTranslator(typeSystem);
        //        IExpression expr = joinTranslator.Convert(outerKeySelector,innerKeySelector,true);
        //        throw new Exception("not implemented");
        //    }
        //    else
        //    {
        //        return Queryable.GroupJoin<T, U, K, V>(this.ToQueryable(), inner.ToQueryable(), outerKeySelector, innerKeySelector, resultSelector);
        //    }
        //}

        #endregion

        public override string ToString()
        {
            return "Table<" + typeof(T).ToString() + "> with condition (" + exprFullySqlTranslatable + ") " + expression;
        }
    }
}
