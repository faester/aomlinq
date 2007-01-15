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

        public void VisitExprIsTrue(ExprIsTrue valueIsTrue){ return; }
        public void VisitExprIsFalse(ExprIsFalse valueIsFalse){ return; }

        public void VisitCstThis(CstThis cstThis){  return; }
        public void VisitProperty(CstProperty vp){ return; }
        public void VisitCstString(CstString cs){ return; }
        public void VisitCstBool(CstBool cb){ return; }
        public void VisitCstLong(CstLong cl){ return; }
        public void VisitCstChar(CstChar ch){ return; }
        public void VisitCstDouble(CstDouble cd){ return; }
        public void VisitCstReference(VarReference cr){ return; }
        public void VisitNestedReference(NestedReference pro){ return; }
        public void VisitOPEquals(OP_Equals eq){ return; }
        public void VisitOPLessThan(OP_LessThan lt){ return; }
        public void VisitOPGreaterThan(OP_GreaterThan gt){ return; }
        public void VisitCstDateTime(CstDateTime cdt){ return; }
        public void VisitEntityPOIDEquals(EntityPOIDEquals epe){ return; }
        public void VisitOPNotEquals(OP_NotEquals ieq){ return; }
        public void VisitInstanceOf(ExprInstanceOf instanceOf){ return; }
        
        public void VisitNotSqlTranslatable(ExprNotTranslatable nts)
        { 
            Console.WriteLine("***************** VisitNotSqlTranslatable");
            hasModifiedExpression = true;
            throw new CannotTranslate(); 
        }

        public void VisitValNotTranslatable(ValNotTranslatable cst)
        {
            Console.WriteLine("**************** VisitValNotTranslatable");
            hasModifiedExpression = true;
            throw new CannotTranslate(); 
        }

    }
}
