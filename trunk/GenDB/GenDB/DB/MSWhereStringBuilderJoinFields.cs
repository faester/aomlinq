using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB.DB
{
    /// <summary>
    /// Antager at Entity tabellen får alias 'e'.
    /// PropertyValue bliver p[PropertyPOID]
    /// </summary>
    class MSWhereStringBuilderJoinFields : IAbsSyntaxVisitor
    {
        StringBuilder wherePart = null;
        int currentPropertyNumber = 0;
        TypeSystem typeSystem = null;
        Dictionary<int, IEntityType> entityTypes = null;

        private MSWhereStringBuilderJoinFields() { /* empty */ }

        public MSWhereStringBuilderJoinFields(TypeSystem typeSystem)
        {
            this.typeSystem = typeSystem;
            Reset();
        }

        public IEnumerable<IEntityType> EntityTypes
        {
            get { return entityTypes.Values; }
        }

        public String WhereStr 
        {
            get {
                return wherePart.ToString();
            }
        }

        private void AppendEntityTypesHaving(IProperty prop)
        {
            IEntityType et = prop.EntityType;

            foreach(IEntityType add in DataContext.Instance.TypeSystem.GetEntityTypesInstanceOf(et))
            {
                entityTypes[add.EntityTypePOID] = add;
            }
        }

        // Leaf
        public void VisitCstThis(CstThis cstThis)
        {
            wherePart.Append (" e.EntityPOID ");
        }

        public void Reset()
        {
            wherePart = new StringBuilder();
            entityTypes = new Dictionary<int, IEntityType>();
        }

        public void Visit(IWhereable clause)
        {
            clause.AcceptVisitor(this);
        }

        public void VisitInstanceOf(ExprInstanceOf instanceOf)
        {
            IEnumerable<IEntityType> types = typeSystem.GetEntityTypesInstanceOf(instanceOf.ClrType);

            foreach(IEntityType et in types)
            {
                entityTypes [et.EntityTypePOID] = et;
            }

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

            if (notFirst) // Test that type was known.
            {
                wherePart.Append(entityTypePoids);
            }
            else
            {   // Unknown type, nothing should be returned.
                wherePart.Append("(0 = 1)");
            }
        }

        //Leaf
        public void VisitProperty(CstProperty vp)
        {
            AppendEntityTypesHaving(vp.Property);
            IProperty p = vp.Property;

            string pvName = "p" + p.PropertyPOID;
            
            switch (p.MappingType )
            {
                case MappingType.BOOL: 
                    wherePart.Append(pvName + ".BoolValue "); break;
                case MappingType.DATETIME: 
                    wherePart.Append(pvName + ".LongValue "); break;
                case MappingType.DOUBLE: 
                    wherePart.Append(pvName + ".DoubleValue "); break;
                case MappingType.LONG: 
                    wherePart.Append(pvName + ".LongValue "); break;
                case MappingType.REFERENCE: 
                    wherePart.Append(pvName + ".ReferenceValue "); break;
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
        public void VisitCstChar(CstChar cs)
        {
            wherePart.Append ('\'');
            wherePart.Append (cs.Ch);
            wherePart.Append ('\'');
            wherePart.Append (' ');
        }

        //Leaf 
        public void VisitNotSqlTranslatable(ExprNotTranslatable cn)
        {
            throw new Exception("IWhereable expression contained nodes that was not SQL-translatable.");
        }

        //Leaf
        public void VisitCstBool(CstBool cb)
        {
            if (cb.Value )
            {
                wherePart.Append ('1');
            }
            else
            {
                wherePart.Append ('0');
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
        public void VisitNestedReference(NestedReference pro)
        {
            AppendEntityTypesHaving(pro.CstProperty.Property);
            IProperty p = null;
            StringBuilder erefSb = new StringBuilder("'");
            NestedReference currentNRef = pro;
            while (currentNRef.InnerReference != null)
            {
                erefSb.Append (currentNRef.CstProperty.Property.PropertyPOID);
                erefSb.Append ('.');
                currentNRef = currentNRef.InnerReference;
            }
            erefSb.Append ("00000");
            erefSb.Append ("'");
            p = currentNRef.CstProperty.Property;
            string pvName = "pv" + currentPropertyNumber;
            
            wherePart.Append ("( SELECT PropertyValue");

            switch (p.MappingType )
            {
                case MappingType.BOOL: 
                    wherePart.Append(".BoolValue "); break;
                case MappingType.DATETIME: 
                    wherePart.Append(".LongValue "); break;
                case MappingType.DOUBLE: 
                    wherePart.Append(".DoubleValue "); break;
                case MappingType.LONG: 
                    wherePart.Append(".LongValue "); break;
                case MappingType.REFERENCE: 
                    wherePart.Append(".ReferenceValue "); break;
                case MappingType.STRING: 
                    wherePart.Append(".StringValue "); break;
                default:
                    throw new Exception("Unknown property mapping.");
            }
            wherePart.Append ("FROM PropertyValue WHERE EntityPOID = ");
            wherePart.Append("dbo.fn_lookup_EntityPOID(e.EntityPOID, ");
            wherePart.Append(erefSb);
            wherePart.Append (") AND ");
            wherePart.Append(pvName);
            wherePart.Append(".PropertyPOID = ");
            wherePart.Append(p.PropertyPOID);
  
            wherePart.Append (") ");
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
                if(neq.Left is VarReference)
                {
                    wherePart.Append (" OR ");
                    neq.Right.AcceptVisitor(this);
                    wherePart.Append(" IS NULL ");
                }
                else if(neq.Right is VarReference)
                {
                    wherePart.Append (" OR ");
                    neq.Left.AcceptVisitor(this);
                    wherePart.Append(" IS NULL ");
                }
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

        public void VisitExprIsTrue(ExprIsTrue csi)
        {
            wherePart.Append ("1 = 1");
        }
        public void VisitExprIsFalse(ExprIsFalse csi)
        {
            wherePart.Append ("1 = 0");
        }
        public void VisitValNotTranslatable(ValNotTranslatable csi)
        {
            throw new Exception("Not SQL-translatable.");
        }
    }
}
