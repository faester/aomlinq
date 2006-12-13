using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    class MSWhereStringBuilder : IAbsSyntaxVisitor
    {
        StringBuilder whereStr = new StringBuilder();

        public String WhereStr 
        {
            get { return whereStr.ToString(); }
        }

        public void Reset()
        {
            whereStr = new StringBuilder();
        }

        public void Visit(IWhereable clause)
        {
            clause.AcceptVisitor(this);
        }

        //Leaf
        public void VisitNumericalProperty(CstProperty vp)
        {
            IProperty p = vp.Property;
            whereStr.Append ("pv.PropertyPOID = ");
            whereStr.Append (p.PropertyPOID);
            whereStr.Append (" AND ");
            switch (p.MappingType )
            {
                case MappingType.BOOL : whereStr.Append ("pv.BoolValue "); break;
                case MappingType.CHAR : whereStr.Append ("pv.CharValue "); break;
                case MappingType.DATETIME : whereStr.Append ("pv.LongValue "); break;
                case MappingType.DOUBLE : whereStr.Append ("pv.DoubleValue "); break;
                case MappingType.LONG : whereStr.Append ("pv.LongValue "); break;
                case MappingType.REFERENCE : whereStr.Append ("pv.LongValue "); break;
                case MappingType.STRING : whereStr.Append ("pv.StringValue "); break;
            }
        }

        //Leaf
        public void VisitCstString(CstString cs)
        {
            whereStr.Append ('\'');
            whereStr.Append (cs.Value);
            whereStr.Append ('\'');
            whereStr.Append (' ');
        }

        //Leaf
        public void VisitCstBool(CstBool cb)
        {
            if (cb.Value )
            {
                whereStr.Append ('0');
            }
            else
            {
                whereStr.Append ('1');
            }
        }

        //Leaf
        public void VisitCstLong(CstLong cl)
        {
            whereStr.Append(cl.Value);
        }

        //Leaf
        public void VisitCstDouble(CstDouble cd)
        {
            whereStr.Append(cd.Value);
        }

        //Leaf
        public void VisitCstReference(VarReference cr)
        {
            if (cr.Value.IsNullReference )
            {
                whereStr.Append (" null ");
            }
            else
            {
                whereStr.Append(cr.Value.EntityPOID);
            }
        }

        //Leaf
        public void VisitPropertyOfReferredObject(PropertyOfReferredObject pro)
        {
            throw new Exception("Not implemented");
        }

        //Node
        public void VisitOPEquals(OP_Equals eq)
        {
            whereStr.Append (" (SELECT DISTINCT EntityPOID FROM PropertyValue WHERE ");
            eq.Left.AcceptVisitor(this);
            whereStr.Append ('=');
            eq.Right.AcceptVisitor (this);
            whereStr.Append(") ");
        }

        public void VisitOPLessThan(OP_LessThan lt)
        {
            whereStr.Append (" (SELECT DISTINCT EntityPOID FROM PropertyValue WHERE ");
            lt.Left.AcceptVisitor(this);
            whereStr.Append ('<');
            lt.Right.AcceptVisitor (this);
            whereStr.Append(") ");
        }

        public void VisitOPGreaterThan(OP_GreaterThan gt)
        {
            whereStr.Append (" (SELECT DISTINCT EntityPOID FROM PropertyValue WHERE ");
            gt.Left.AcceptVisitor(this);
            whereStr.Append ('>');
            gt.Right.AcceptVisitor (this);
            whereStr.Append(") ");
        }

        public void VisitNotExpr(ExprNot expr)
        {
            whereStr.Append (" EXCEPT ( ");
            expr.Expression.AcceptVisitor(this);
            whereStr.Append (") ");
        }

        public void VisitAndExpr(ExprAnd expr)
        {
            whereStr.Append (" (");
            expr.Left.AcceptVisitor (this);
            whereStr.Append (") INTERSECT (");
            expr.Right.AcceptVisitor (this);
            whereStr.Append (") ");
        }

        public void VisitOrExpr(ExprOr expr)
        {
            whereStr.Append (" (");
            expr.Left.AcceptVisitor (this);
            whereStr.Append (") UNION (");
            expr.Right.AcceptVisitor (this);
            whereStr.Append (") ");
        }
        
        public void VisitCstDateTime(CstDateTime cdt)
        {
            whereStr.Append(cdt.Value.Ticks);
        }
    }
}
