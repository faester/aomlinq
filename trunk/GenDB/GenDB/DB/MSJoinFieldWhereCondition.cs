using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB.DB
{
    /// <summary>
    /// Will translate an abstract where condition to a format that can 
    /// be used with the MSSQL2005-server. 
    /// <br/>
    /// The result can be obtained using the WhereStr-property. The string 
    /// returned by this property is not useful in it selt, but is a part 
    /// of the query strategy in JoinPropertyIterator.
    /// <br/>
    /// The name might be slightly misleading, but alludes to the 
    /// strategy used to create the query text.
    /// <br/> 
    /// (Visitor pattern.)
    /// </summary>
    class MSJoinFieldWhereCondition : IAbsSyntaxVisitor
    {
        /// <summary>
        /// Stores the query expression built by this visitor.
        /// </summary>
        StringBuilder wherePart = null;

        /// <summary>
        /// The TypeSystem instance used when building the query.
        /// It is necessary to have access to the type system in 
        /// order to make requests for subclasses of a type etc.
        /// </summary>
        TypeSystem typeSystem = null;

        /// <summary>
        /// Stores the IEntityType instances that is present in
        /// the abstract wherecondition.
        /// </summary>
        Dictionary<int, IEntityType> entityTypes = null;

        /// <summary>
        /// Stores the IProperty instances that is present in
        /// the abstract wherecondition.
        /// </summary>
        Dictionary<IProperty, int> properties = null;

        private MSJoinFieldWhereCondition() { /* empty */ }


        public MSJoinFieldWhereCondition(TypeSystem typeSystem)
        {
            this.typeSystem = typeSystem;
            Reset();
        }

        /// <summary>
        /// Returns the IEntityTypes encountered in the
        /// last IExpression visited.
        /// </summary>
        public IEnumerable<IEntityType> EntityTypes
        {
            get { return entityTypes.Values; }
        }

        /// <summary>
        /// Returns the IProperty instances encountered 
        /// in the last IExpression visited.
        /// </summary>
        public IEnumerable<IProperty> Properties
        {
            get { return properties.Keys; }
        }

        /// <summary>
        /// Returns the SQL-translation of the 
        /// last visited IExpression.
        /// References to PropertyValue gets aliases
        /// p[PropertyPOID].
        /// </summary>
        public String WhereStr 
        {
            get {
                string res = wherePart.ToString();
                Console.WriteLine(res);
                return wherePart.ToString();
            }
        }

        /// <summary>
        /// Used internally to store information
        /// of the IProperty instances encountered
        /// when visiting nodes/leafs of an IExpression.
        /// </summary>
        /// <param name="prop"></param>
        private void RegisterProperty(IProperty prop)
        {
            if (!properties.ContainsKey(prop))
            {
                properties.Add(prop, 0);
            }
            properties[prop] += 1;
            IEntityType et = prop.EntityType;

            foreach(IEntityType add in DataContext.Instance.TypeSystem.GetEntityTypesInstanceOf(et))
            {
                entityTypes[add.EntityTypePOID] = add;
            }
        }

        /// <summary>
        /// Resets the visitor and prepares it 
        /// for visiting a new IExpression. Should be 
        /// called before visiting new IExpressions. 
        /// Is called by the constructor, so it is not 
        /// necessary to call it before IExpression visit.
        /// </summary>
        public void Reset()
        {
            wherePart = new StringBuilder();
            properties = new Dictionary<IProperty, int>();
            entityTypes = new Dictionary<int, IEntityType>();
        }

        /// <summary>
        /// Leaf. Not ment to be called externally.
        /// </summary>
        /// <param name="cstThis"></param>
        public void VisitCstThis(CstThis cstThis)
        {
            wherePart.Append (" e.EntityPOID ");
        }

        /// <summary>
        /// Commences new IExpression visit. 
        /// Only method to be invoked externally.
        /// <br/>
        /// If the clause contains a ValNotTranslatable
        /// an exception will be thrown.
        /// </summary>
        /// <param name="clause"></param>
        public void Visit(IWhereable clause)
        {
            clause.AcceptVisitor(this);
        }


        public void VisitArithmeticOperator(ArithmeticOperator ao)
        {
            // MSSql server performs automatic casting of types, but will
            // attempt to cast to INT, if a VARCHAR is added to an integer.
            // This does not correspond to the behaviour in C#, so when 
            // an LINQ Expression uses operator '+' to add a string and a 
            // numerical value, we must ensure that the numerical is cast
            // to a VARCHAR.
            bool strCastLeft = false;
            bool strCastRight = false;

            if (ao.LeftIsString ^ ao.RightIsString) // If Left and Right are of same mapping type no casts are needed.
            {
                if (ao.LeftIsString) {
                    strCastRight = true;
                }
                else
                {
                    strCastLeft = true;
                }
            }

            wherePart.Append ('(');
            if (strCastLeft) { wherePart.Append ("CAST ("); }
            Visit(ao.Left);
            if (strCastLeft) { wherePart.Append (" AS VARCHAR(MAX))"); }
            wherePart.Append(ao.OperatorSymbol);
            if (strCastRight) { wherePart.Append ("CAST ("); }
            Visit(ao.Right);
            if (strCastRight) { wherePart.Append (" AS VARCHAR(MAX))"); }
            wherePart.Append (')');
        }

        /// <summary>
        /// Not ment to be invoked externally. (Node)
        /// </summary>
        /// <param name="instanceOf"></param>
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


        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// </summary>
        /// <param name="vp"></param>
        public void VisitProperty(CstProperty vp)
        {
            RegisterProperty(vp.Property);

            string pvName = "p" + vp.Property.PropertyPOID;
            wherePart.Append (pvName);
            IProperty p = vp.Property;
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
                    throw new Exception("Unknown property mapping."); // Should never happen unless new MappingTypes are introduced
            }
            wherePart.Append (' ');
        }

        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// </summary>
        /// <param name="cs"></param>
        public void VisitCstString(CstString cs)
        {
            wherePart.Append ('\'');
            wherePart.Append(MsSql2005DB.SqlSanitizeString(cs.Value));
            wherePart.Append ('\'');
            wherePart.Append (' ');
        }

        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// </summary>
        /// <param name="cs"></param>
        public void VisitCstChar(CstChar cs)
        {
            string charString = MsSql2005DB.SqlSanitizeString(cs.Ch.ToString());
            wherePart.Append ('\'');
            wherePart.Append (charString);
            wherePart.Append ('\'');
            wherePart.Append (' ');
        }

        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// </summary>
        /// <param name="cn"></param>
        public void VisitNotSqlTranslatable(ExprNotTranslatable cn)
        {
            throw new Exception("IWhereable expression contained nodes that was not SQL-translatable.");
        }

        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// </summary>
        /// <param name="cb"></param>
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

        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// 
        /// </summary>
        /// <param name="cl"></param>
        public void VisitCstLong(CstLong cl)
        {
            wherePart.Append(cl.Value);
        }

        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// 
        /// </summary>
        /// <param name="cd"></param>
        public void VisitCstDouble(CstDouble cd)
        {

            string dStr = cd.Value.ToString().Replace(',', '.');
            wherePart.Append(dStr);
        }

        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// 
        /// </summary>
        /// <param name="cr"></param>
        public void VisitCstReference(VarReference cr)
        {
            if (cr.Value.IsNullReference )
            {
                wherePart.Append (" null ");
            }
            else
            {
                IEntityType et = typeSystem.GetEntityType(cr.TypeOfReference);
                entityTypes[et.EntityTypePOID] = et;
                wherePart.Append(cr.Value.EntityPOID);
            }
        }

        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// 
        /// </summary>
        /// <param name="pro"></param>
        public void VisitNestedReference(NestedReference pro)
        {
            RegisterProperty(pro.CstProperty.Property);
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
            string pvName = "nested" + p.PropertyPOID;

            wherePart.Append("(SELECT ");
            switch (p.MappingType)
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
            wherePart.Append(" FROM PropertyValue ");
            wherePart.Append(pvName);
            wherePart.Append(" WHERE ");
            wherePart.Append(pvName);
            wherePart.Append(".PropertyPOID = ");
            wherePart.Append(p.PropertyPOID);
            wherePart.Append(" AND ");
            wherePart.Append(pvName);
            wherePart.Append(".EntityPOID = dbo.fn_lookup_EntityPOID(e.EntityPOID, ");
            wherePart.Append(erefSb);
            wherePart.Append(") )");
        }

        /// <summary>
        /// Not ment to be called externally. (Node)
        /// 
        /// </summary>
        /// <param name="eq"></param>
        public void VisitOPEquals(BoolEquals eq)
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
        
        /// <summary>
        /// Not ment to be called externally. (Node)
        /// </summary>
        /// <param name="neq"></param>
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
 
        /// <summary>
        /// Not ment to be called externally. (Node)
        /// 
        /// </summary>
        /// <param name="expr"></param>
        public void VisitNotExpr(ExprNot expr)
        {
            wherePart.Append (" NOT ( ");
            expr.Expression.AcceptVisitor(this);
            wherePart.Append (") ");
        }

        /// <summary>
        /// Not ment to be called externally. (Node)
        /// 
        /// </summary>
        /// <param name="lt"></param>
        public void VisitOPLessThan(BoolLessThan lt)
        {
            lt.Left.AcceptVisitor(this);
            wherePart.Append ('<');
            lt.Right.AcceptVisitor (this);
        }

        /// <summary>
        /// Not ment to be called externally. (Node)
        /// 
        /// </summary>
        /// <param name="gt"></param>
        public void VisitOPGreaterThan(BoolGreaterThan gt)
        {
            gt.Left.AcceptVisitor(this);
            wherePart.Append ('>');
            gt.Right.AcceptVisitor (this);
        }

        /// <summary>
        /// Not ment to be called externally. (Node)
        /// 
        /// </summary>
        /// <param name="expr"></param>
        public void VisitAndExpr(ExprAnd expr)
        {
            wherePart.Append (" (");
            expr.Left.AcceptVisitor (this);
            wherePart.Append (") AND (");
            expr.Right.AcceptVisitor (this);
            wherePart.Append (") ");
        }

        /// <summary>
        /// Not ment to be called externally. (Node)
        /// 
        /// </summary>
        /// <param name="expr"></param>
        public void VisitOrExpr(ExprOr expr)
        {
            wherePart.Append (" (");
            expr.Left.AcceptVisitor (this);
            wherePart.Append (") OR (");
            expr.Right.AcceptVisitor (this);
            wherePart.Append (") ");
        }

        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// 
        /// </summary>
        /// <param name="cdt"></param>
        public void VisitCstDateTime(CstDateTime cdt)
        {
            wherePart.Append(cdt.Value.Ticks);
        }

        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// 
        /// </summary>
        /// <param name="csi"></param>
        public void VisitExprIsTrue(ExprIsTrue csi)
        {
            wherePart.Append ("1 = 1");
        }

        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// 
        /// </summary>
        /// <param name="csi"></param>
        public void VisitExprIsFalse(ExprIsFalse csi)
        {
            wherePart.Append ("1 = 0");
        }

        /// <summary>
        /// Not ment to be called externally. (Leaf)
        /// If this method is called during traversal 
        /// og the IExpression an Exception will be thrown. 
        /// <br/>
        /// The SqlExprTranslator may insert ValNotTranslatable 
        /// instances in the IExpression generated, if it 
        /// encounters nodes in the Linq Expression, that it 
        /// can not map to the IExpression language. In this 
        /// case these elements should be removed before the
        /// IExpression is parsed. (The SqlExprChecker can 
        /// perform the operation of removing untranslatable 
        /// expression while ensuring, that the result set of 
        /// the original Linq expression will always be a subset
        /// of the result set returned by the database constrained
        /// by the IExpression the SqlExprChecked produces.)
        /// </summary>
        /// <param name="csi"></param>
        public void VisitValNotTranslatable(ValNotTranslatable csi)
        {
            throw new Exception("Not SQL-translatable.");
        }
    }
}
