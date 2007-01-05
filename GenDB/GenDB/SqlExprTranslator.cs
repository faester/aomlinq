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
            return Visit(expr);
        }

        internal IExpression VisitMethodCall(MethodCallExpression mce)
        {
            ReadOnlyCollection<Expression> roc = mce.Parameters;
            IValue[] parArr= new IValue[2];

            if(roc.Count==2) 
            {   
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
                else
                {
                    throw new Exception("NodeType unknown");
                }
            }
            else
            {
                throw new Exception("Can not translate method with more than two parameters");
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
                parArr[0] = VisitUnaryExpression((UnaryExpression)be.Left);
            }

            // doing the right side
            if(be.Right.ToString()=="null")
                parArr[1] = new VarReference(null);
            else if(be.Right is ConstantExpression)
            {
                switch(TypeSystem.FindMappingType(expr.Type))
                {
                case MappingType.BOOL:
                    parArr[1] = new CstLong(System.Convert.ToInt64(be.Right.ToString()));
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
                catch(Exception e)
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
            //throw new Exception("STOP");

            if(nodeType=="GT")
                return new GenDB.OP_GreaterThan(parArr[0], parArr[1]);
            else if(nodeType=="LT")
                return new GenDB.OP_LessThan(parArr[0], parArr[1]);
            else if(nodeType=="EQ")
                return new GenDB.OP_Equals (parArr[0], parArr[1]);
            else if(nodeType=="NE")
                return new GenDB.OP_NotEquals(parArr[0], parArr[1]);
            else
                throw new Exception("NodeType unknown "+expr.NodeType.ToString());
        }

        internal IValue VisitUnaryExpression(UnaryExpression ue)
        {
            string exprType = ue.Operand.GetType().Name;
            if(exprType=="MemberExpression")
            {
                MemberExpression me = (MemberExpression)ue.Operand;
                return VisitMemberExpression(me);
            }
            else if(exprType=="ConstantExpression")
            {
                ConstantExpression ce = (ConstantExpression) ue.Operand;
                Type t = ce.Type;

                if(!TypeSystem.IsTypeKnown(t))
                    TypeSystem.RegisterType(t);

                IEntityType et = TypeSystem.GetEntityType(t);
                
                throw new Exception("not implemented: "+ue.Operand.GetType().Name);
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
                    string typeName;
                    
                    // doing the left hand side
                    typeName = be.Left.GetType().Name;
                    if(be.Left.GetType().Name.ToString()=="BinaryExpression")
                    {
                        BinaryExpression b_tmp = (BinaryExpression) be.Left;
                        left = VisitBinaryExpression(b_tmp);
                    }
                    else if(typeName=="MethodCallExpression")
                    {
                        MethodCallExpression m_tmp = (MethodCallExpression) be.Left;
                        left = VisitMethodCall(m_tmp);
                    }
                    else
                        throw new Exception("Expression type unknown "+be.Left.GetType().Name);

                    // doing the right hand side
                    typeName = be.Right.GetType().Name;
                    if(typeName=="BinaryExpression")
                    {
                        BinaryExpression b_tmp = (BinaryExpression) be.Right;
                        right = VisitBinaryExpression(b_tmp);
                    } 
                    else if(typeName=="MethodCallExpression")
                    {
                        MethodCallExpression m_tmp = (MethodCallExpression) be.Right;
                        right = VisitMethodCall(m_tmp);
                    }
                    else 
                        throw new Exception("Expression type unknown "+typeName);
                    
                    return new GenDB.ExprAnd(left, right);
                
                }
                else if(mecstr.StartsWith("OrElse("))
                {
                    LambdaExpression le = (LambdaExpression) expr;
                    BinaryExpression be = (BinaryExpression) le.Body;
                    IExpression left, right;
                    string typeName;

                    // doing the left hand side
                    typeName = be.Left.GetType().Name;
                    if(typeName=="BinaryExpression")
                    {
                        BinaryExpression b_tmp = (BinaryExpression) be.Left;
                        left = VisitBinaryExpression(b_tmp);
                    }
                    else if(typeName=="MethodCallExpression")
                    {
                        MethodCallExpression m_tmp = (MethodCallExpression) be.Left;
                        left = VisitMethodCall(m_tmp);
                    }
                    else
                        throw new Exception("Expression type unkown "+typeName);
                   
                    // doing the right hand side
                    typeName = be.Right.GetType().Name;
                    if(typeName=="BinaryExpression")
                    {
                        BinaryExpression b_tmp = (BinaryExpression) be.Right;
                        right = VisitBinaryExpression(b_tmp);
                    }
                    else if(typeName=="MethodCallExpression")
                    {
                        MethodCallExpression m_tmp = (MethodCallExpression) be.Right;
                        right = VisitMethodCall(m_tmp);
                    }
                    else
                        throw new Exception("Expression type unkown "+typeName);
 
                    return new GenDB.ExprOr(left, right);
                }
                else if(mecstr.StartsWith("GE("))
                {
                    throw new Exception("not implemented");
                }
                else
                {
                    throw new Exception("Can not translate method name " + mecstr);
                }
            }
            else 
                throw new Exception("sd");
        }

        internal IWhereable VisitEqExpr(Expression exp)
        {
            
            return VisitExpr(exp);
            throw new Exception("not implemented");
        }
        
        internal IWhereable VisitMemberAccess(MemberExpression m)
        { 
            Expression expression2 = (Expression)Visit(m.Expression);
            //if (expression2 != m.Expression)
            //{
            //    return ExpressionVisitor.MakeMemberExpression(expression2, m.Member);
            //}
            throw new Exception("STOP");
            //return Visit(m);
        }

        internal IWhereable VisitParameter(ParameterExpression p)
        {
            Type t = p.Type;
            
            IEntityType et = TypeSystem.GetEntityType(t);
            
            Console.WriteLine("EntityType: {0}", et.Name);
            IProperty po = et.GetProperty("Name");
            //Console.WriteLine("PropertyTypeName: {0}",po.PropertyType.Name);
            //return new CstProperty();
            throw new Exception("STOP");
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

        internal IWhereable Visit(Expression exp)
        //internal Expression Visit(Expression exp)
        {

            if (exp == null)
            {
                Console.WriteLine("Call to Visit: NodeType=null");
                return null;
            }
            Console.WriteLine("Call to Visit: {0}",exp.NodeType);

            switch (exp.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                        ExceptionThrower (exp);
                        break;
                case ExpressionType.AndAlso:
                    // do stuff
                    throw new Exception("AndAlso Exception");
                    //return new ExprAnd( TranslateAndAlso(), TranslateAndAlso());
                case ExpressionType.BitwiseAnd:
                case ExpressionType.BitwiseOr:
                case ExpressionType.BitwiseXor:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                    ExceptionThrower(exp);
                        break;
                case ExpressionType.EQ:
                    // do stuff
                    VisitEqExpr((BinaryExpression)exp);
                    //VisitLambdaExpr((LambdaExpression)exp);
                    throw new Exception("EQ Exception");
                case ExpressionType.GT:
                    // do stuff
                    throw new Exception("GT Exception");
                case ExpressionType.GE:
                case ExpressionType.Index:
                case ExpressionType.LE:
                case ExpressionType.LShift:
                    ExceptionThrower(exp);
                    break;
                case ExpressionType.LT:
                    // do stuff
                    throw new Exception("LT Exception");
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NE:
                case ExpressionType.Or:
                    ExceptionThrower(exp);
                    break;
                case ExpressionType.OrElse:
                    // do stuff
                    throw new Exception("OrElse Exception");
                case ExpressionType.RShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    //{
                    //    return this.VisitBinary((BinaryExpression)exp);
                    //}
                case ExpressionType.As:
                case ExpressionType.BitwiseNot:
                case ExpressionType.Cast:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Len:
                case ExpressionType.Negate:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                    //{
                    //    return this.VisitUnary((UnaryExpression)exp);
                    //}
                case ExpressionType.Conditional:
                    //{
                    //    return this.VisitConditional((ConditionalExpression)exp);
                    //}
                case ExpressionType.Constant:
                    //{
                    //    return this.VisitConstant((ConstantExpression)exp);
                    //}
                case ExpressionType.Funclet:
                    //{
                    //    return this.VisitFunclet((FuncletExpression)exp);
                    //}
                case ExpressionType.Invoke:
                    //{
                    //    return this.VisitInvocation((InvocationExpression)exp);
                    //}
                case ExpressionType.Is:
                    ExceptionThrower(exp);
                    break;
                    //{
                    //    return this.VisitTypeIs((TypeBinaryExpression)exp);
                    //}
                case ExpressionType.Lambda:
                    return VisitExpr(exp);
                    //{
                    //    return this.VisitLambda((LambdaExpression)exp);
                    //}
                case ExpressionType.ListInit:
                    ExceptionThrower(exp);
                    break;
                    //{
                    //    return this.VisitListInit((ListInitExpression)exp);
                    //}
                case ExpressionType.MemberAccess:
                    Console.Write("");
                    return VisitMemberAccess((MemberExpression)exp);
                    //{
                    //    return this.VisitMemberAccess((MemberExpression)exp);
                    //}
                case ExpressionType.MemberInit:
                    //{
                    //    return this.VisitMemberInit((MemberInitExpression)exp);
                    //}
                case ExpressionType.MethodCall:
                case ExpressionType.MethodCallVirtual:
                    throw new Exception("not implemented");
                    //return VisitMethodCallExpr((MethodCallExpression)exp);
                    //{
                    //    return this.VisitMethodCall((MethodCallExpression)exp);
                    //}
                case ExpressionType.New:
                    //{
                    //    return this.VisitNew((NewExpression)exp);
                    //}
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    //{
                    //    return this.VisitNewArray((NewArrayExpression)exp);
                    //}
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression) exp);
                    //Console.Write("");
                    
                    //{
                    //    return this.VisitParameter((ParameterExpression)exp);
                    //}
            }
            throw new InvalidOperationException(string.Format("Unhandled Expression Type: {0}", exp.NodeType));
        }
        #endregion

    }
}
