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
                foreach (IEntity ie in db.Where(expression))
                {
                    iboCache.Remove(ie.EntityPOID);
                }
                db.ClearWhere(expression);
            }
            else
            {
                foreach (IEntity ie in db.Where(expression))
                {
                    IIBoToEntityTranslator trans = translators.GetTranslator(ie.EntityType.EntityTypePOID);

                    T deleteCandidate = (T)trans.Translate (ie);
                    if (linqFunc(deleteCandidate))
                    {
                        iboCache.Remove(ie.EntityPOID);
                        db.ClearWhere(new OP_Equals (new VarReference(deleteCandidate), CstThis.Instance));
                    }
                }
            }
        }


        /// <summary>
        /// Returns true, if the element given exists in the database.
        /// Equality is resolved solely on the EntityPOID of the objects.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool Contains(T e)
        {
            if (e == null) { return false;}
            if (e.DBTag == null) { return false; }
            IExpression where = new OP_Equals(new VarReference(e), CstThis.Instance);
            foreach (IEntity ibo in db.Where(where))
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
            if (e.DBTag == null) { return false; }
            IWhereable where = new OP_Equals(new VarReference(e), CstThis.Instance);
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
                    foreach(IEntity e in db.Where(expression))
                    {
                        IIBoToEntityTranslator trans = translators.GetTranslator(e.EntityType.EntityTypePOID);
                        T test = (T)trans.Translate (e);
                        if (linqFunc(test)) { count++; }
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
            IEntityType lastType = null;
            IIBoToEntityTranslator translator = null;
            Console.WriteLine("System.Collections.Generic.IEnumerator<T> GetEnumerator()");
            foreach (IEntity e in db.Where(expression))
            {
                Console.WriteLine("***");
                if (lastType != e.EntityType)
                {
                    translator = translators.GetTranslator (e.EntityType.EntityTypePOID);
                }
                T res = (T)translator.Translate (e);
                if (exprFullySqlTranslatable)
                {
                    yield return res;
                }
                else
                {
                    if (linqFunc(res))
                    {
                        Console.WriteLine("+++");
                        yield return res;
                    }
                    else
                    {
                        Console.WriteLine("---");
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
            Console.WriteLine("Nu vil den gerne lave en Table med T = " + typeof(T));

            Table<T> res = new Table<T>();
            
            res.translators = this.translators ;
            res.typeSystem = this.typeSystem ;
            res.db = this.db;
            res.iboCache = this.iboCache;
            
            SqlExprTranslator exprTranslator = new SqlExprTranslator(typeSystem);
            SqlExprChecker checker = new SqlExprChecker();
            IExpression sqlExpr = new ExprAnd( exprTranslator.Convert (expr), this.expression);
            Console.WriteLine("F�R: " + sqlExpr);
            checker.StartVisit(sqlExpr);
            Console.WriteLine("EFTER: " + sqlExpr);
            res.expression = sqlExpr;
            
            res.exprFullySqlTranslatable = (!checker.HasModifiedExpression) && this.exprFullySqlTranslatable;
            Console.WriteLine("NY TABEL: " + res.exprFullySqlTranslatable + " " + res.expression);
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

        #endregion

        public override string ToString()
        {
            return "Table<" + typeof(T).ToString() + "> with condition " + expression;
        }
    }
}
