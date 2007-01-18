using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Query;
using System.Expressions;
using GenDB.DB;

namespace GenDB
{
    internal class SqlJoinTranslator
    {
        TypeSystem typeSystem;

        internal SqlJoinTranslator(TypeSystem typeSystem)
        {
            this.typeSystem = typeSystem;
        }

        public IExpression Convert(Expression outer, Expression inner)
        {
            IExpression left = VisitExpr(outer);
            IExpression right = VisitExpr(inner);
            return new ExprAnd(left,right);
        }

        internal IExpression VisitExpr(Expression expr)
        {
            throw new Exception("not implemented");
        }
    }
}
