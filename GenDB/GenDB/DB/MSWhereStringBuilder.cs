using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB.DB
{
    class MSWhereStringBuilder : IAbsSyntaxVisitor
    {
        StringBuilder selectPart = null;
        StringBuilder wherePart = null;
        StringBuilder joinPart = null;
        int currentPropertyNumber = 0;

        public MSWhereStringBuilder()
        {
            Reset();
        }

        public String WhereStr 
        {
            get {
                StringBuilder res = new StringBuilder(selectPart.Length + wherePart.Length + joinPart.Length + 30);
                res.Append(selectPart);
                res.Append (" \n\tWHERE (");
                res.Append (wherePart );
                if (joinPart.Length > 0)
                {
                    res.Append(") AND (");
                    res.Append(joinPart);
                }
                res.Append (")");

                return res.ToString();
            }
        }

        public void Reset()
        {
            selectPart = new StringBuilder("SELECT DISTINCT e.EntityPOID FROM Entity e");
            wherePart = new StringBuilder();
            joinPart = new StringBuilder();
        }

        public void Visit(IWhereable clause)
        {
            clause.AcceptVisitor(this);
        }

        public void VisitInstanceOf(ExprInstanceOf instanceOf)
        {
            IEnumerable<IEntityType> types = TypeSystem.GetEntityTypesInstanceOf(instanceOf.ClrType);
            bool notFirst = false;

            StringBuilder entityTypePoids = new StringBuilder("e.EntityTypePOID IN (");

            // Construct list of EntityTypePOIDs
            foreach (IEntityType et in types)
            {
                if (notFirst) { entityTypePoids.Append(','); }
                else { notFirst = true; }
                entityTypePoids.Append (et.EntityTypePOID);
            }

            entityTypePoids.Append (")");

            wherePart.Append (entityTypePoids);
        }

        //Leaf
        public void VisitNumericalProperty(CstProperty vp)
        {

            string pvName = "pv" + currentPropertyNumber;
            selectPart.Append (", PropertyValue " + pvName);
            IProperty p = vp.Property;
            if (currentPropertyNumber > 0)
            {
                joinPart.Append(" AND ");
            }
            joinPart.Append(pvName );
            joinPart.Append(".EntityPOID = e.EntityPOID AND ");
            joinPart.Append(pvName);
            joinPart.Append(".PropertyPOID = ");
            joinPart.Append(p.PropertyPOID);
            switch (p.MappingType )
            {
                case MappingType.BOOL: 
                    wherePart.Append(pvName + ".BoolValue "); break;
                case MappingType.CHAR: 
                    wherePart.Append(pvName + ".CharValue "); break;
                case MappingType.DATETIME: 
                    wherePart.Append(pvName + ".LongValue "); break;
                case MappingType.DOUBLE: 
                    wherePart.Append(pvName + ".DoubleValue "); break;
                case MappingType.LONG: 
                    wherePart.Append(pvName + ".LongValue "); break;
                case MappingType.REFERENCE: 
                    wherePart.Append(pvName + ".LongValue "); break;
                case MappingType.STRING: 
                    wherePart.Append(pvName + ".StringValue "); break;
                default:
                    throw new Exception("Unknown property mapping.");
            }
            wherePart.Append (' ');
            currentPropertyNumber++;
        }

        //Leaf
        public void VisitCstString(CstString cs)
        {
            wherePart.Append ('\'');
            wherePart.Append (cs.Value);
            wherePart.Append ('\'');
            wherePart.Append (' ');
        }

        //Leaf
        public void VisitCstBool(CstBool cb)
        {
            if (cb.Value )
            {
                wherePart.Append ('0');
            }
            else
            {
                wherePart.Append ('1');
            }
        }

        //Leaf
        public void VisitCstLong(CstLong cl)
        {
            wherePart.Append(cl.Value);
        }

        //Leaf
        public void VisitCstDouble(CstDouble cd)
        {
            wherePart.Append(cd.Value);
        }

        //Leaf
        public void VisitCstReference(VarReference cr)
        {
            if (cr.Value.IsNullReference )
            {
                wherePart.Append (" null ");
            }
            else
            {
                wherePart.Append(cr.Value.EntityPOID);
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
            bool leftIsNullReference=false;
            bool rightIsNullReference=false;
            if(eq.Left is VarReference )
            {
                VarReference lv = (VarReference) eq.Left;    
                if(lv.Value.IsNullReference)
                {
                    leftIsNullReference = true;
                }
            }    
            if(eq.Right is VarReference)
            {                                  
                VarReference rv = (VarReference) eq.Right;
                if(rv.Value.IsNullReference)
                {
                    rightIsNullReference=true;
                }
            }

            if(!leftIsNullReference && !rightIsNullReference)
            {
                eq.Left.AcceptVisitor (this);
                wherePart.Append ('=');
                eq.Right.AcceptVisitor (this);
            } 
            else if(leftIsNullReference)
            {
                eq.Right.AcceptVisitor(this);
                wherePart.Append("IS NULL");
            } 
            else if(rightIsNullReference)
            {
                eq.Left.AcceptVisitor(this);
                wherePart.Append("IS NULL");
            }
        }
        
        public void VisitOPNotEquals(OP_NotEquals neq)
        {
            bool leftIsNullReference=false;
            bool rightIsNullReference=false;
            if(neq.Left is VarReference)
            {
                VarReference lv = (VarReference) neq.Left;
                if(lv.Value.IsNullReference)
                {
                    leftIsNullReference=true;
                }
            }
            if(neq.Right is VarReference)
            {
                VarReference rv = (VarReference) neq.Right;
                if(rv.Value.IsNullReference)
                {
                    rightIsNullReference=true;
                }
            }

            if(!leftIsNullReference && !rightIsNullReference)
            {
                neq.Left.AcceptVisitor(this);
                wherePart.Append (" <> ");
                neq.Right.AcceptVisitor (this);
                wherePart.Append (" OR ");
                neq.Left.AcceptVisitor(this);
                wherePart.Append(" IS NULL ");
            }
            else if(leftIsNullReference)
            {
                wherePart.Append(" NOT ");
                neq.Right.AcceptVisitor (this);
                wherePart.Append(" IS NULL ");
            }
            else if(rightIsNullReference)
            {
                wherePart.Append(" NOT ");
                neq.Left.AcceptVisitor(this);
                wherePart.Append(" IS NULL ");
            }

            
        }
        
        public void VisitNotExpr(ExprNot expr)
        {
            wherePart.Append (" NOT ( ");
            expr.Expression.AcceptVisitor(this);
            wherePart.Append (") ");
        }


        public void VisitOPLessThan(OP_LessThan lt)
        {
            lt.Left.AcceptVisitor(this);
            wherePart.Append ('<');
            lt.Right.AcceptVisitor (this);
        }

        public void VisitOPGreaterThan(OP_GreaterThan gt)
        {
            gt.Left.AcceptVisitor(this);
            wherePart.Append ('>');
            gt.Right.AcceptVisitor (this);
        }



        public void VisitAndExpr(ExprAnd expr)
        {
            wherePart.Append (" (");
            expr.Left.AcceptVisitor (this);
            wherePart.Append (") AND (");
            expr.Right.AcceptVisitor (this);
            wherePart.Append (") ");
        }

        public void VisitOrExpr(ExprOr expr)
        {
            wherePart.Append (" (");
            expr.Left.AcceptVisitor (this);
            wherePart.Append (") OR (");
            expr.Right.AcceptVisitor (this);
            wherePart.Append (") ");
        }
        
        public void VisitCstDateTime(CstDateTime cdt)
        {
            wherePart.Append(cdt.Value.Ticks);
        }

        //Leaf
        public void VisitEntityPOIDEquals(EntityPOIDEquals epe)
        {
            wherePart.Append("e.EntityPOID = ");
            wherePart.Append(epe.EntityPOID);
        }

        public void VisitCstIsTrue(CstIsTrue csi)
        {
            wherePart.Append ("1 = 1");
        }
        public void VisitCstIsFalse(CstIsFalse csi)
        {
            wherePart.Append ("1 = 0");
        }
    }
}
