using System;
using System.Collections.Generic;
using System.Text;
using GenDB.AbstractSyntax;

namespace GenDB
{
    /// <summary>
    /// This class is used to check if a given abstract expression 
    /// is translatable to SQL. 
    /// <para>
    /// The class modifies the visited clause in a manner that makes
    /// it possible to translate the resulting clause to SQL. The
    /// set described by the element clause will always be a subset
    /// of the result clause.
    /// </para>
    /// <para>
    /// This class is intended to be used to determine and aid distributed
    /// evaluation of the clause between the SQL-server and in memory query 
    /// methods, when the clause can not be expressed in SQL alone.
    /// </para>
    /// </summary>
    class SqlExprChecker : IAbsSyntaxVisitor
    {        
        class CannotTranslate : Exception {}

        bool hasModifiedExpression = false;

        /// <summary>
        /// Returns true if this visitor has modified the 
        /// last visited clause.
        /// </summary>
        public bool HasModifiedExpression
        {
            get { return hasModifiedExpression; }
        }

        /// <summary>
        /// Delegate used to move subbranches to a higher level 
        /// in the clause tree when appropriate.
        /// </summary>
        /// <param name="expr"></param>
        delegate void SetParentExpression(IExpression expr);

        SetParentExpression parentSetter = null;

        /// <summary>
        /// This class is used to check if a given abstract expression 
        /// is translatable to SQL. If the clause checked is fully 
        /// translatable the expression will be left unmodified. 
        /// If the clause is not completely translatable, the clause
        /// will be modified, such that every untranslatable element
        /// is replaced as desdribed below:
        /// <para>
        /// Furthermore subbranches of untranslatable subexpressions 
        /// will be removed and if the untranslatable expression is 
        /// a branch in an OR-expression, the entire OR-expression is 
        /// substituted by a CstIsTrue. If clause is a branch in an 
        /// AND-expression, that branch is replaced by a CstIsTrue 
        /// instance.
        /// </para>
        /// <para>
        /// The purpose of the said substitutions is to ensure, that 
        /// the set described by the element clause is a subset of 
        /// the set described by the result clause.
        /// </para>
        /// </summary>
        /// <param name="clause"></param>
        public void StartVisit(IWhereable clause)
        {
            parentSetter = delegate(IExpression expr) { clause = expr; };
            Visit(clause);
        }

        public void Visit(IWhereable clause)
        { 
            clause.AcceptVisitor(this);
        }

        public void VisitNotExpr(ExprNot expr) {
            parentSetter = delegate(IExpression e) { expr.Expression = e; };
            Visit(expr.Expression);
        }


        public void VisitAndExpr(ExprAnd expr){ 
            try {
                parentSetter = delegate(IExpression e) { expr.Left = e; };
                Visit(expr.Left);
            }
            catch(CannotTranslate)
            {
                expr.Left = ExprIsTrue.Instance;
                parentSetter = null;
            }
            try {
                parentSetter = delegate(IExpression e) { expr.Right = e; };
                Visit(expr.Right);
            }
            catch(CannotTranslate)
            {
                expr.Right = ExprIsTrue.Instance;
                parentSetter = null;
            }
        }
        
        public void VisitOrExpr(ExprOr expr){ 
            try {
                parentSetter = delegate(IExpression e) { expr.Left = e; };
                Visit(expr.Left);
            }
            catch(CannotTranslate)
            {
                parentSetter (ExprIsTrue.Instance); 
                return;
            }
            try {
                parentSetter = delegate(IExpression e) { expr.Right = e; };
                Visit(expr.Right);
            }
            catch(CannotTranslate)
            {
                parentSetter (ExprIsTrue.Instance);
            }
        }

        public void VisitArithmeticOperator(ArithmeticOperator ao)
        {
            Visit(ao.Left);
            Visit(ao.Right);
        }

        //Leaf
        public void VisitExprIsTrue(ExprIsTrue valueIsTrue){ return; }

        //Leaf
        public void VisitExprIsFalse(ExprIsFalse valueIsFalse){ return; }

        //Leaf
        public void VisitCstThis(CstThis cstThis){  return; }

        //Leaf
        public void VisitProperty(CstProperty vp){ return; }

        //Leaf
        public void VisitCstString(CstString cs){ return; }

        //Leaf
        public void VisitCstBool(CstBool cb){ return; }

        //Leaf
        public void VisitCstLong(CstLong cl){ return; }

        //Leaf
        public void VisitCstChar(CstChar ch){ return; }

        //Leaf
        public void VisitCstDouble(CstDouble cd){ return; }

        //Leaf
        public void VisitCstReference(VarReference cr){ return; }

        //Leaf
        public void VisitNestedReference(NestedReference pro){ return; }

        //Leaf
        public void VisitCstDateTime(CstDateTime cdt) { return; }

        ////Leaf
        //public void VisitEntityPOIDEquals(EntityPOIDEquals epe) { return; }
        
        public void VisitOPEquals(BoolEquals eq)
        { 
            Visit(eq.Left);
            Visit(eq.Right);
        }

        public void VisitOPLessThan(BoolLessThan lt)
        { 
            Visit(lt.Left);
            Visit(lt.Right);
        }

        public void VisitOPGreaterThan(BoolGreaterThan gt)
        { 
            Visit(gt.Left);
            Visit(gt.Right);
        }


        public void VisitOPNotEquals(OP_NotEquals ieq)
        { 
            Visit(ieq.Left);
            Visit(ieq.Right);
        }

        //Leaf
        public void VisitInstanceOf(ExprInstanceOf instanceOf) 
        {
            return;
        }
        
        public void VisitNotSqlTranslatable(ExprNotTranslatable nts)
        { 
            hasModifiedExpression = true;
            throw new CannotTranslate(); 
        }

        public void VisitValNotTranslatable(ValNotTranslatable cst)
        {
            hasModifiedExpression = true;
            throw new CannotTranslate(); 
        }

    }
}
