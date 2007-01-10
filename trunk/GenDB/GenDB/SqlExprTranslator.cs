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
    // internal class fra System.Query.dll, sakset via reflector
    
    internal class SqlExprTranslator
    {
        internal SqlExprTranslator()
        {
        }

        public IWhereable Convert(Expression expr)
        {
            return VisitExpr(expr);
        }

        internal IExpression VisitMethodCall(MethodCallExpression mce)
        {
            ReadOnlyCollection<Expression> roc = mce.Parameters;
            IValue[] parArr= new IValue[2];
 
            if(roc[0].NodeType.ToString()=="MemberAccess")
            {
                MemberExpression tmp = (MemberExpression)roc[0];                    
                Type t = tmp.Expression.Type;
                
                if(!TypeSystem.IsTypeKnown(t))
                    TypeSystem.RegisterType (t);
                        
                IEntityType et = TypeSystem.GetEntityType(t);
                string propstr = tmp.Member.Name;
                IProperty po = et.GetProperty(propstr);

                parArr[0] = new CstProperty(po);
                    
                switch(TypeSystem.FindMappingType(roc[1].Type))
                {
                    case MappingType.STRING:
                        parArr[1] = new CstString(roc[1].ToString().Trim('"'));
                        break;
                    case MappingType.DATETIME:
                        ConstantExpression ce = (ConstantExpression)roc[1];
                        parArr[1] = new CstDateTime((DateTime)ce.Value);
                        break;
                    default:
                        throw new Exception("type not implemented "+TypeSystem.FindMappingType(roc[1].Type));
                }
                    
                if(mce.Method.Name=="op_Equality")
                    return new GenDB.OP_Equals (parArr[0], parArr[1]);
                else if(mce.Method.Name=="op_Inequality")
                    return new GenDB.OP_NotEquals(parArr[0], parArr[1]);
                else 
                    throw new Exception("Method unknown "+mce.Method.Name);
            }
            else if(roc[0].NodeType.ToString()=="Constant")
            {
              //  ConstantExpression ce
                throw new Exception("not implemented");
            }
            else
            {
                throw new Exception("NodeType unknown: "+roc[0].NodeType.ToString());
            }
        }

        internal IExpression VisitBinaryExpression(BinaryExpression be)
        {
            Expression expr = (Expression) be;
            IValue[] parArr = new IValue[2];

            // doing the left side
            if(be.Left is MemberExpression)
            {
                parArr[0] = VisitMemberExpression((MemberExpression) be.Left);
            }
            else if(be.Left is UnaryExpression)
            {
                parArr[0] = VisitUnaryExpressionValue((UnaryExpression)be.Left);
            }
            // doing the right side
            if(be.Right.ToString()=="null")
                parArr[1] = new VarReference(null);
            else if(be.Right is ConstantExpression)
            {
                switch(TypeSystem.FindMappingType(expr.Type))
                {
                case MappingType.BOOL:
                    switch(TypeSystem.FindMappingType(be.Right.Type))
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

                        default:
                            throw new Exception("Unknown MappingType: "+TypeSystem.FindMappingType(be.Right.Type));
                    }
                        break;

                default:
                    throw new Exception("type not implemented "+expr.Type);
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
                    parArr[1] = CstIsFalse.Instance;
                }
                //parArr[1] = VisitUnaryExpression((UnaryExpression)be.Right);
            }
            else if(be.Right is MethodCallExpression)
            {
                parArr[1] = (IValue)VisitMethodCall((MethodCallExpression)be.Right);
                throw new Exception("not implemented: "+be.Right);
            }
            else
            {
                throw new Exception("Unknown Type of Right side: "+be.Right.ToString());
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
            else
                throw new Exception("NodeType unknown "+expr.NodeType.ToString());
        }

        internal IExpression VisitUnaryExpression(UnaryExpression ue)
        {
            throw new Exception("not implemented");
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
                return new CstThis();
            }
            else
            {
                throw new Exception("Unknown type: "+ue.Operand.GetType().Name);
            }
        }

        internal IValue VisitMemberExpression(MemberExpression me)
        {
            Type t = me.Expression.Type;

            if(!TypeSystem.IsTypeKnown(t))
                TypeSystem.RegisterType(t);

            IEntityType et = TypeSystem.GetEntityType(t);
            string propstr = me.Member.Name;
            IProperty po = et.GetProperty(propstr);
            return new CstProperty(po);
        }
        
        public IExpression VisitExpr(Expression expr)
        {
            if(expr.NodeType.ToString()=="Lambda")
            {
                LambdaExpression lambda = (LambdaExpression)expr;
                string mecstr = lambda.Body.ToString();

                if (mecstr.StartsWith("op_Equality(") || mecstr.StartsWith("op_Inequality"))
                {
                    return VisitMethodCall((MethodCallExpression) lambda.Body);                
                }
                else if(mecstr.StartsWith("EQ(") || mecstr.StartsWith("GT(") || mecstr.StartsWith("LT(") || mecstr.StartsWith("NE("))
                {
                    return VisitBinaryExpression((BinaryExpression) lambda.Body);
                }
                else if(mecstr.StartsWith("AndAlso("))
                {
                    BinaryExpression be = (BinaryExpression) lambda.Body;
                    IExpression left, right;
                    
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
                    else
                        throw new Exception("Expression type unknown "+be.Left.GetType().Name);

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
                    else 
                        throw new Exception("Expression type unknown "+be.Right.ToString());
                    
                    return new GenDB.ExprAnd(left, right);
                
                }
                else if(mecstr.StartsWith("OrElse("))
                {
                    BinaryExpression be = (BinaryExpression) lambda.Body;
                    IExpression left, right;
                    
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
                    else
                        throw new Exception("Expression type unkown "+be.Left.ToString());
                   
                    // doing the right hand side
                    if(be.Right is BinaryExpression)
                    {
                        right = VisitBinaryExpression((BinaryExpression) be.Right);
                    }
                    else if(be.Right is MethodCallExpression)
                    {
                        right = VisitMethodCall((MethodCallExpression) be.Right);
                    }
                    else
                        throw new Exception("Expression type unkown "+be.Right.ToString());
 
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
                        throw new Exception("Unknown Operand.NodeType: "+ue.Operand.NodeType.ToString());
                }
                else if(lambda.Body is MemberExpression)
                {
                    MemberExpression me = (MemberExpression)lambda.Body;
                    switch(TypeSystem.FindMappingType(me.Type))
                    {
                        case MappingType.BOOL:
                            return VisitBooleanMember(me, true);
                            break;

                        default:
                            throw new Exception("MappingType not implemented: "+TypeSystem.FindMappingType(me.Type));
                    }   
                }
                else
                {
                    throw new Exception("Can not translate method name " + mecstr);
                }
            }
            else if(expr.NodeType.ToString()=="Not")
            {
                UnaryExpression ue = (UnaryExpression) expr;
                return new GenDB.ExprNot(VisitBinaryExpression((BinaryExpression)ue.Operand));
            }
            else 
                throw new Exception("unknown expression type: "+expr);
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
      
        internal IExpression VisitBooleanMember(MemberExpression me, bool equality)
        {
            ParameterExpression pe = (ParameterExpression)me.Expression;
            string name = me.Member.Name;

            IValue[] parArr = new IValue[2];
            parArr[0] = VisitMemberExpression(me);
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
#if DEBUG
            Console.WriteLine("MakeBinaryExpression\n");
#endif
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

        internal static MemberExpression MakeMemberExpression(Expression expr, MemberInfo mi)
        {
#if DEBUG
            Console.WriteLine("MakeMemberExpression\n");
#endif
            FieldInfo info3 = mi as FieldInfo;
            if (info3 != null)
            {
                return Expression.Field(expr, info3);
            }
            PropertyInfo info4 = mi as PropertyInfo;
            if (info4 == null)
            {
                throw new Exception("Member is not a Field or Property: " + mi);
            }
            return Expression.Property(expr, info4);
        }

        internal static MethodCallExpression MakeMethodCallExpression(ExpressionType eType, Expression obj, MethodInfo method, IEnumerable<Expression> args)
        {
            switch (eType)
            {
                case ExpressionType.MethodCall:
                    {
                        return Expression.Call(method, obj, args);
                    }
                case ExpressionType.MethodCallVirtual:
                    {
                        return Expression.CallVirtual(method, obj, args);
                    }
            }
            throw new ArgumentException("eType: " + eType);
        }

        internal static UnaryExpression MakeUnaryExpression(ExpressionType eType, Expression operand, Type type)
        {
#if DEBUG
            Console.WriteLine("MakeUnaryExpression\n");
#endif
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

        private void ExceptionThrower(Expression e)
        {
            throw new Exception("Cannot translate " + e.ToString());
        }

        #region VisitNodeMethods

        //internal IWhereable Visit(Expression exp)
        ////internal Expression Visit(Expression exp)
        //{
        //    if (exp == null)
        //    {
        //        Console.WriteLine("Call to Visit: NodeType=null");
        //        return null;
        //    }
            
        //    switch (exp.NodeType)
        //    {
        //        case ExpressionType.Add:
        //        case ExpressionType.AddChecked:
        //        case ExpressionType.And:
        //        case ExpressionType.AndAlso:
        //        case ExpressionType.BitwiseAnd:
        //        case ExpressionType.BitwiseOr:
        //        case ExpressionType.BitwiseXor:
        //        case ExpressionType.Coalesce:
        //        case ExpressionType.Divide:
        //        case ExpressionType.EQ:
        //            //VisitEqExpr((BinaryExpression)exp);
        //            //break;

        //        case ExpressionType.GT:
        //        case ExpressionType.GE:
        //        case ExpressionType.Index:
        //            throw new Exception("not implemented");
        //        case ExpressionType.LE:
        //            throw new Exception("LE not implemented");
        //        case ExpressionType.LShift:
        //        case ExpressionType.LT:
        //        case ExpressionType.Modulo:
        //        case ExpressionType.Multiply:
        //        case ExpressionType.MultiplyChecked:
        //        case ExpressionType.NE:
        //        case ExpressionType.Or:
        //        case ExpressionType.OrElse:
        //        case ExpressionType.RShift:
        //        case ExpressionType.Subtract:
        //        case ExpressionType.SubtractChecked:
        //        case ExpressionType.As:
        //        case ExpressionType.BitwiseNot:
        //        case ExpressionType.Cast:
        //        case ExpressionType.Convert:
        //        case ExpressionType.ConvertChecked:
        //        case ExpressionType.Len:
        //        case ExpressionType.Negate:
        //        case ExpressionType.Not:
        //        case ExpressionType.Quote:
        //        case ExpressionType.Conditional:
        //        case ExpressionType.Constant:
        //        case ExpressionType.Funclet:
        //        case ExpressionType.Invoke:
        //        case ExpressionType.Is:
        //            throw new Exception("not implemented");
        //            ExceptionThrower(exp);
                    
        //        case ExpressionType.Lambda:
        //            return VisitExpr(exp);

        //        case ExpressionType.ListInit:
        //        case ExpressionType.MemberAccess:
        //        case ExpressionType.MemberInit:
        //        case ExpressionType.MethodCall:
        //        case ExpressionType.MethodCallVirtual:
        //        case ExpressionType.New:
        //        case ExpressionType.NewArrayInit:
        //        case ExpressionType.NewArrayBounds:
        //            throw new Exception("not implemented");
                
        //    }
        //    throw new InvalidOperationException(string.Format("Unhandled Expression Type: {0}", exp.NodeType));
        //}
        #endregion

    }
}
