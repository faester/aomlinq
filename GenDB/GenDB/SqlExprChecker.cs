using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    class SqlExprChecker : IAbsSyntaxVisitor
    {        
        class CannotTranslate : Exception {}

        bool hasModifiedExpression = false;

        public bool HasModifiedExpression
        {
            get { return hasModifiedExpression; }
        }

        delegate void SetParentExpression(IExpression expr);

        SetParentExpression parentSetter = null;

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
        
        public void VisitOPEquals(OP_Equals eq)
        { 
            Visit(eq.Left);
            Visit(eq.Right);
        }

        public void VisitOPLessThan(OP_LessThan lt)
        { 
            Visit(lt.Left);
            Visit(lt.Right);
        }

        public void VisitOPGreaterThan(OP_GreaterThan gt)
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
