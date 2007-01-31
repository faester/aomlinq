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
    public class FieldsNotAllowedInConditionsException : Exception
    {
        public FieldsNotAllowedInConditionsException()
            : base("The value of Public fields can not be guarenteed and therefore are not allowed")
        { }
    }

    internal class SqlExprTranslator
    {
        TypeSystem typeSystem; 

        internal SqlExprTranslator(TypeSystem typeSystem)
        {
            this.typeSystem = typeSystem;
        }

        public IExpression Convert(Expression expr)
        {
            return VisitExpr(expr);
        }

        internal int GetNumParamFromMember(MemberExpression me)
        {
            string analyseThis = me.ToString();
            int c=0;
            for(int i=0; i<analyseThis.Length;i++)
            {
                if(analyseThis.ElementAt(i)=='.')
                    c++;
            }
            return c;
        }
        
        internal NestedReference GetNestedRefs(MemberExpression me, int size)
        {
            MemberExpression mTmp;
            NestedReference nref=null;
            NestedReference nTmp=null;
            //if(size>1)
            //{
                for(int i=0; i<size; i++)
                {
                    mTmp=me;
                    Type type = mTmp.Expression.Type;
                    if(!typeSystem.IsTypeKnown(type))
                    {
                            typeSystem.RegisterType(type);
                    }

                    IEntityType et = typeSystem.GetEntityType(type);
                    IProperty prop = et.GetProperty(mTmp.Member.Name);
                    nref = new NestedReference(nTmp,new CstProperty(prop));
                    nTmp = nref;
                    if(mTmp.Expression.NodeType.ToString() == "MemberAccess")
                        me = (MemberExpression)mTmp.Expression;
                }
            //}            
            return nref;
        }

        internal IExpression VisitMethodCall(MethodCallExpression mce)
        {
            ReadOnlyCollection<Expression> roc = mce.Parameters;
            IValue[] parArr= new IValue[2];

            if(roc.Count==2)
            {
                if(roc[0] is MemberExpression)
                {
                    MemberExpression tmp = (MemberExpression)roc[0];  
                    parArr[0] = VisitMemberExpression(tmp);
                    
                    switch(typeSystem.FindMappingType(roc[1].Type))
                    {
                        case MappingType.STRING:
                            parArr[1] = new CstString(roc[1].ToString().Trim('"'));
                            break;
                        case MappingType.DATETIME:
                            ConstantExpression ce = (ConstantExpression)roc[1];
                            parArr[1] = new CstDateTime((DateTime)ce.Value);
                            break;
                        default:
                            parArr[1] = ValNotTranslatable.Instance;
                            break;
                    }
                        
                    if(mce.Method.Name=="op_Equality")
                        return new GenDB.OP_Equals (parArr[0], parArr[1]);
                    else if(mce.Method.Name=="op_Inequality")
                        return new GenDB.OP_NotEquals(parArr[0], parArr[1]);
                    else 
                        return ExprNotTranslatable.Instance;
                }
                else
                {
                    return GenDB.ExprNotTranslatable.Instance;
                }
            } 
            else
            {
                return ExprNotTranslatable.Instance;
            }
        }

        internal IExpression VisitBinaryExpression(BinaryExpression be)
        {
            Expression expr = (Expression) be;
            IValue[] parArr = new IValue[2];
            IExpression left = null, right = null;
            MemberExpression me;
            // doing the left side
            if(be.Left is MemberExpression)
            {
                me = (MemberExpression)be.Left;
                if(IsMemberAPublicField(me))
                    throw new FieldsNotAllowedInConditionsException();
                else
                    parArr[0] = VisitMemberExpression((MemberExpression) be.Left);
            }
            else if(be.Left is UnaryExpression) 
            {
                parArr[0] = VisitUnaryExpressionValue((UnaryExpression)be.Left);
            }
            else if(be.Left is MethodCallExpression)
            {   
                if(be.Left.NodeType.ToString()=="MethodCallVirtual")
                    return ExprNotTranslatable.Instance;
                else
                    left = VisitMethodCall((MethodCallExpression) be.Left);
            }
            else if(be.Left is BinaryExpression)
            {
                left = VisitExpr(be.Left);
            }
            else
                parArr[0] = ValNotTranslatable.Instance;

            // doing the right side
            if(be.Right.ToString()=="null")
                parArr[1] = new VarReference(null);
            else if(be.Right is ConstantExpression)
            {
                switch(typeSystem.FindMappingType(expr.Type))
                {
                case MappingType.BOOL:
                    switch(typeSystem.FindMappingType(be.Right.Type))
                    {
                        case MappingType.BOOL:
                            ConstantExpression ce = (ConstantExpression)be.Right;
                            parArr[1] = new GenDB.CstBool((bool)ce.Value);
                            break;

                        case MappingType.LONG:
                            parArr[1] = new CstLong(System.Convert.ToInt64(be.Right.ToString()));
                            break;

                        case MappingType.DOUBLE:
                            parArr[1] = new CstDouble(System.Convert.ToDouble(be.Right.ToString()));
                            break;
                    }
                        break;
                default:
                    parArr[1] = ValNotTranslatable.Instance;
                    break;
                }
            }
            else if(be.Right is UnaryExpression)
            {
                UnaryExpression un = (UnaryExpression) be.Right;
                ConstantExpression ce = (ConstantExpression) un.Operand;   
                IBusinessObject ib;
                try
                {
                    ib = (IBusinessObject) ce.Value;
                    parArr[1] = new VarReference(ib);
                } 
                catch(Exception)
                {
                    return ExprIsFalse.Instance;
                }
            }
            else if(be.Right is MethodCallExpression)
            {
                right = VisitMethodCall((MethodCallExpression) be.Right);
            }
            else
            {
                parArr[1] = ValNotTranslatable.Instance;
            }
            
            string nodeType = expr.NodeType.ToString();
            if(nodeType=="GT")
            {
                return new GenDB.OP_GreaterThan(parArr[0], parArr[1]);
            }
            else if(nodeType=="LT")
            {
                return new GenDB.OP_LessThan(parArr[0], parArr[1]);
            }
            else if(nodeType=="EQ")
            {
                return new GenDB.OP_Equals (parArr[0], parArr[1]);
            }
            else if(nodeType=="NE")
            {
                return new GenDB.OP_NotEquals(parArr[0], parArr[1]);
            }
            else if(nodeType=="GE")
            {
                return new GenDB.OP_GreaterThan(parArr[0], parArr[1]);
            }
            else if(nodeType=="OrElse")
            {
                return new GenDB.ExprOr(left, right);
            }
            else if(nodeType=="AndAlso")
            {
                return new GenDB.ExprAnd(left, right);
            }
            else
                return ExprNotTranslatable.Instance;
        }

        internal IValue VisitUnaryExpressionValue(UnaryExpression ue)
        {
            if(ue.Operand is MemberExpression)
            {
                MemberExpression me = (MemberExpression)ue.Operand;
                return VisitMemberExpression(me);
            }
            else if(ue.Operand is ParameterExpression)
            {
                return CstThis.Instance;
            }
            else
            {
                return ValNotTranslatable.Instance;
            }
        }

        internal IValue VisitMemberExpression(MemberExpression me)
        {
            // REFLECTED TYPES
            int iPar = GetNumParamFromMember(me);
            
            // LAST TYPE IN FOODCHAIN
            Type t = me.Expression.Type;
            if(!typeSystem.IsTypeKnown(t) )
            {
                if(TranslatorChecks.ImplementsIBusinessObject(t))
                    typeSystem.RegisterType(t);
                else
                    return ValNotTranslatable.Instance;
            }
            if(IsMemberAPublicField(me))
                throw new FieldsNotAllowedInConditionsException();
            else
            {
                if(iPar>1)
                {
                    return GetNestedRefs(me,iPar);  
                }
                else
                {
                    IEntityType et = typeSystem.GetEntityType(t);
                    string propstr = me.Member.Name;
                    IProperty po = et.GetProperty(propstr);
                    return new CstProperty(po);
                }
            }
        }
        
        public IExpression VisitExpr(Expression expr)
        {
            IExpression left, right;
            if(expr.NodeType.ToString()=="Lambda")
            {
                LambdaExpression lambda = (LambdaExpression)expr;
                string mecstr = lambda.Body.ToString();

                if (mecstr.StartsWith("op_Equality(") || mecstr.StartsWith("op_Inequality"))
                {
                    IValue[] val = new IValue[2];
                    
                    return VisitMethodCall((MethodCallExpression) lambda.Body);                
                }
                else if(mecstr.StartsWith("EQ(") || mecstr.StartsWith("GT(") || mecstr.StartsWith("LT(") || mecstr.StartsWith("NE("))
                {
                    return VisitBinaryExpression((BinaryExpression) lambda.Body);
                }
                else if(mecstr.StartsWith("AndAlso("))
                {
                    BinaryExpression be = (BinaryExpression) lambda.Body;
                    
                    // doing the left hand side
                    if(be.Left is BinaryExpression)
                    {
                        BinaryExpression b_tmp = (BinaryExpression) be.Left;
                        left = VisitBinaryExpression(b_tmp);
                    }
                    else if(be.Left is MethodCallExpression)
                    {
                        MethodCallExpression m_tmp = (MethodCallExpression) be.Left;
                        left = VisitMethodCall(m_tmp);
                    }
                    else if(be.Left is MemberExpression)
                    {
                        MemberExpression left_me = (MemberExpression)be.Left;
                        left = DecomposeCompositeMemberExpression(left_me);
                    }
                    else if(be.Left is UnaryExpression)
                    {
                        UnaryExpression sd = (UnaryExpression) be.Left;
                        left = VisitBooleanMember((MemberExpression)sd.Operand, false);
                    }
                    else
                        left = ExprNotTranslatable.Instance;

                    // doing the right hand side
                    if(be.Right is BinaryExpression)
                    {
                        BinaryExpression b_tmp = (BinaryExpression) be.Right;
                        right = VisitBinaryExpression(b_tmp);
                    } 
                    else if(be.Right is MethodCallExpression)
                    {
                        MethodCallExpression m_tmp = (MethodCallExpression) be.Right;
                        right = VisitMethodCall(m_tmp);
                    }
                    else if(be.Right is MemberExpression)
                    {
                        MemberExpression right_me = (MemberExpression)be.Right;
                        right = DecomposeCompositeMemberExpression(right_me);
                    }
                    else if(be.Right is UnaryExpression)
                    {
                        bool boolOp=false;
                        if(be.Left.NodeType.ToString() != "Not")
                            boolOp=true;

                        UnaryExpression sd = (UnaryExpression) be.Right;
                        right = VisitBooleanMember((MemberExpression)sd.Operand, boolOp);
                    }
                    else 
                        right = ExprNotTranslatable.Instance; 
                    
                    return new GenDB.ExprAnd(left, right);
                }
                else if(mecstr.StartsWith("OrElse("))
                {
                    BinaryExpression be = (BinaryExpression) lambda.Body;
                    
                    // doing the left hand side  
                    if(be.Left is BinaryExpression)
                    {
                        BinaryExpression b_tmp = (BinaryExpression) be.Left;
                        left = VisitBinaryExpression(b_tmp);
                    }
                    else if(be.Left is MethodCallExpression)
                    {
                        MethodCallExpression m_tmp = (MethodCallExpression) be.Left;
                        left = VisitMethodCall(m_tmp);
                    }
                    else if(be.Left is MemberExpression)
                    {
                        MemberExpression left_me = (MemberExpression)be.Left;
                        left = DecomposeCompositeMemberExpression(left_me);
                    }
                    else if(be.Left is UnaryExpression)
                    {
                        bool boolOp=false;
                        if(be.Left.NodeType.ToString() != "Not")
                            boolOp=true;

                        UnaryExpression sd = (UnaryExpression) be.Left;
                        left = VisitBooleanMember((MemberExpression)sd.Operand, boolOp);   
                    }
                    else
                        left = ExprNotTranslatable.Instance;
                   
                    // doing the right hand side
                    if(be.Right is BinaryExpression)
                    {
                        right = VisitBinaryExpression((BinaryExpression) be.Right);
                    }
                    else if(be.Right is MethodCallExpression)
                    {
                        right = VisitMethodCall((MethodCallExpression) be.Right);
                    }
                    else if(be.Right is MemberExpression)
                    {
                        MemberExpression right_me = (MemberExpression)be.Right;
                        right = DecomposeCompositeMemberExpression(right_me);
                    }
                    else if(be.Right is UnaryExpression)
                    {
                        bool boolOp=false;
                        if(be.Left.NodeType.ToString() != "Not")
                            boolOp=true;

                        UnaryExpression sd = (UnaryExpression) be.Right;
                        right = VisitBooleanMember((MemberExpression)sd.Operand, boolOp);
                    }
                    else
                        right = ExprNotTranslatable.Instance;
 
                    return new GenDB.ExprOr(left, right);
                }
                else if(mecstr.StartsWith("GE("))
                { // = !(x < y)
                    return DecomposeQueryGE(expr);
                }
                else if(mecstr.StartsWith("LE("))
                { // = !(x > y) 
                    return DecomposeQueryLE(expr);
                }
                else if(mecstr.StartsWith("Not("))
                {
                    UnaryExpression ue = (UnaryExpression)lambda.Body;
                    
                    if(ue.Operand.NodeType.ToString() == "EQ" || ue.Operand.NodeType.ToString() == "NE")
                    {
                        return VisitExpr(ue);
                    }
                    else if(ue.Operand.NodeType.ToString() == "GE")
                    {
                        return DecomposeQueryGE(ue);
                    }
                    else if(ue.Operand.NodeType.ToString() == "LE")
                    {
                        return DecomposeQueryLE(ue);
                    }
                    else if(ue.Operand.NodeType.ToString() == "GT" || ue.Operand.NodeType.ToString() == "LT")
                    {
                        return VisitExpr(ue);
                    }
                    else if(ue.Operand.NodeType.ToString() == "MethodCall")
                    {
                        return new GenDB.ExprNot(VisitMethodCall((MethodCallExpression)ue.Operand));
                    }
                    else if(ue.Operand.NodeType.ToString() == "MemberAccess")
                    {
                        return VisitBooleanMember((MemberExpression)ue.Operand, false);
                    }
                    else
                        return ExprNotTranslatable.Instance;
                }
                else if(lambda.Body is MemberExpression)
                {
                    MemberExpression me = (MemberExpression)lambda.Body;
                    switch(typeSystem.FindMappingType(me.Type))
                    {
                        case MappingType.BOOL:
                            return VisitBooleanMember(me, true);
                            break;

                        default:
                            return ExprNotTranslatable.Instance;
                    }   
                }
                else
                {
                    return ExprNotTranslatable.Instance;
                }
            }
            else if(expr.NodeType.ToString()=="Not")
            {
                UnaryExpression ue = (UnaryExpression) expr;
                if(ue.Operand is BinaryExpression)
                {
                    return new GenDB.ExprNot(VisitBinaryExpression((BinaryExpression)ue.Operand));
                } 
                else if(ue.Operand is MemberExpression)
                {
                    //return new ExprNot(Vis
                    throw new Exception("nor implemented");
                }
                else 
                {
                    throw new Exception("unknown UnaryExpression Operand");
                }
            }
            else if(expr.NodeType.ToString()=="OrElse")
            {   
                BinaryExpression be = (BinaryExpression) expr;
                left = VisitExpr(be.Left);
                right = VisitExpr(be.Right);
                return new GenDB.ExprOr(left,right);
            }
            else if(expr.NodeType.ToString()=="MethodCall")
            {
                return VisitMethodCall((MethodCallExpression)expr);
            }
            else 
                return ExprNotTranslatable.Instance;
        }

        internal IExpression DecomposeCompositeMemberExpression(MemberExpression me)
        {
            bool boolOp=false;
            if(me.NodeType.ToString() != "Not")
                boolOp=true;

            switch(typeSystem.FindMappingType(me.Type))
            {
                case MappingType.BOOL:
                    return VisitBooleanMember(me, boolOp);
            }
            return ExprNotTranslatable.Instance;
        }

        internal IExpression DecomposeQueryLE(Expression expr)
        {
            LambdaExpression lambda = (LambdaExpression) expr;
            BinaryExpression be = (BinaryExpression)lambda.Body;
            UnaryExpression ue = MakeUnaryExpression(ExpressionType.Not, BinaryExpression.GT(be.Left,be.Right), expr.Type);
            return VisitExpr(ue);
        }

        internal IExpression DecomposeQueryLE(UnaryExpression ue)
        {
            BinaryExpression operand = (BinaryExpression)ue.Operand;
            BinaryExpression be = MakeBinaryExpression(ExpressionType.GT, operand.Left, operand.Right);
            return VisitBinaryExpression(be);
        }

        internal IExpression DecomposeQueryGE(Expression expr)
        {
            LambdaExpression lambda = (LambdaExpression) expr;
            BinaryExpression be = (BinaryExpression)lambda.Body;
            UnaryExpression ue = MakeUnaryExpression(ExpressionType.Not, BinaryExpression.LT(be.Left,be.Right), expr.Type);
            return VisitExpr(ue);
        }

        internal IExpression DecomposeQueryGE(UnaryExpression ue)
        {
            BinaryExpression operand = (BinaryExpression)ue.Operand;
            BinaryExpression be = MakeBinaryExpression(ExpressionType.LT, operand.Left, operand.Right);
            return VisitBinaryExpression(be);
        }

        internal bool IsMemberAPublicField(MemberExpression me)
        {
            if(me.Member.MemberType == MemberTypes.Field)
            {
                return true;
            }
            return false;
        }
      
        internal IExpression VisitBooleanMember(MemberExpression me, bool equality)
        {
            IValue[] parArr = new IValue[2];
            if(me.Expression.NodeType.ToString()=="Parameter")
            {
                ParameterExpression pe = (ParameterExpression)me.Expression;
                string name = me.Member.Name;
                parArr[0] = VisitMemberExpression(me);
            }
            else if(me.Member.MemberType == MemberTypes.Property )
            {
                parArr[0] = GetNestedRefs(me,GetNumParamFromMember(me));
            }
            else
            {
                parArr[0] = ValNotTranslatable.Instance;
            }
                
            if(equality)
                    parArr[1] = new GenDB.CstBool(true);
                else
                    parArr[1] = new GenDB.CstBool(false);

                IExpression ie = new GenDB.OP_NotEquals(parArr[0],parArr[1]);            
                return new GenDB.ExprNot(ie);
        }

        #region MakeTreeMethods

        internal static BinaryExpression MakeBinaryExpression(ExpressionType eType, Expression left, Expression right)
        {
            switch (eType)
            {
                case ExpressionType.Add:
                    {
                        return Expression.Add(left, right);
                    }
                case ExpressionType.AddChecked:
                    {
                        return Expression.AddChecked(left, right);
                    }
                case ExpressionType.And:
                    {
                        return Expression.And(left, right);
                    }
                case ExpressionType.AndAlso:
                    {
                        return Expression.AndAlso(left, right);
                    }
                case ExpressionType.BitwiseAnd:
                    {
                        return Expression.BitAnd(left, right);
                    }
                case ExpressionType.BitwiseOr:
                    {
                        return Expression.BitOr(left, right);
                    }
                case ExpressionType.BitwiseXor:
                    {
                        return Expression.BitXor(left, right);
                    }
                case ExpressionType.Coalesce:
                    {
                        return Expression.Coalesce(left, right);
                    }
                case ExpressionType.Divide:
                    {
                        return Expression.Divide(left, right);
                    }
                case ExpressionType.EQ:
                    {
                        return Expression.EQ(left, right);
                    }
                case ExpressionType.GT:
                    {
                        return Expression.GT(left, right);
                    }
                case ExpressionType.GE:
                    {
                        return Expression.GT(left, right);
                    }
                case ExpressionType.Index:
                    {
                        return Expression.Index(left, right);
                    }
                case ExpressionType.LE:
                    {
                        return Expression.LE(left, right);
                    }
                case ExpressionType.LShift:
                    {
                        return Expression.LShift(left, right);
                    }
                case ExpressionType.LT:
                    {
                        return Expression.LT(left, right);
                    }
                case ExpressionType.Modulo:
                    {
                        return Expression.Modulo(left, right);
                    }
                case ExpressionType.Multiply:
                    {
                        return Expression.Multiply(left, right);
                    }
                case ExpressionType.MultiplyChecked:
                    {
                        return Expression.MultiplyChecked(left, right);
                    }
                case ExpressionType.NE:
                    {
                        return Expression.NE(left, right);
                    }
                case ExpressionType.Or:
                    {
                        return Expression.Or(left, right);
                    }
                case ExpressionType.OrElse:
                    {
                        return Expression.OrElse(left, right);
                    }
                case ExpressionType.RShift:
                    {
                        return Expression.RShift(left, right);
                    }
                case ExpressionType.Subtract:
                    {
                        return Expression.Subtract(left, right);
                    }
                case ExpressionType.SubtractChecked:
                    {
                        return Expression.SubtractChecked(left, right);
                    }
            }
            throw new ArgumentException("eType: " + eType);
        }

        internal static UnaryExpression MakeUnaryExpression(ExpressionType eType, Expression operand, Type type)
        {
            ExpressionType type1 = eType;
            if (type1 <= ExpressionType.ConvertChecked)
            {
                switch (type1)
                {
                    case ExpressionType.As:
                        {
                            return Expression.As(operand, type);
                        }
                    case ExpressionType.BitwiseAnd:
                        {
                            goto Label_0093;
                        }
                    case ExpressionType.BitwiseNot:
                        {
                            return Expression.BitNot(operand);
                        }
                    case ExpressionType.Cast:
                        {
                            return Expression.Cast(operand, type);
                        }
                    case ExpressionType.Convert:
                        {
                            return Expression.Convert(operand, type);
                        }
                    case ExpressionType.ConvertChecked:
                        {
                            return Expression.ConvertChecked(operand, type);
                        }
                }
            }
            else if (type1 <= ExpressionType.Negate)
            {
                if (type1 == ExpressionType.Len)
                {
                    return Expression.Len(operand);
                }
                if (type1 == ExpressionType.Negate)
                {
                    return Expression.Negate(operand);
                }
            }
            else if (type1 != ExpressionType.Not)
            {
                if (type1 == ExpressionType.Quote)
                {
                    return Expression.Quote(operand);
                }
            }
            else
            {
                return Expression.Not(operand);
            }
        Label_0093:
            throw new ArgumentException("eType: " + eType);
        }
        #endregion
    }
}