using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Query;
using System.Expressions;

namespace GenDB
{
    // internal class fra System.Query.dll, sakset via reflector

    internal class MSSqlExprTranslator
    {
        internal MSSqlExprTranslator()
        {
        }

        public IWhereable Convert(Expression expr)
        {
            //throw new Exception("asldkj");
            return Visit(expr);
        }
        
        public IWhereable VisitLambdaExpr(LambdaExpression lambda)
        {
            Console.Write("");
            return Visit(lambda.Body);
        }

        public IWhereable VisitMethodCallExpr(MethodCallExpression m)
        {  
            Console.WriteLine(m.Parameters[0]);
            Console.WriteLine("decType: {0}",m.Method.DeclaringType);
            
            Expression expression2 = m.Object;
            IEnumerable<Expression> enumerable2 = this.VisitExpressionList(m.Parameters);
            
            Console.Write("");
            //if ((expression2 == m.Object) && (enumerable2 == m.Parameters))
            //{
            //    return Visit(m);
            //}
            
            MethodCallExpression mce = MakeMethodCallExpression(m.NodeType, expression2, m.Method, enumerable2);
            return Visit(mce);
        }

        internal IEnumerable<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> list2 = null;
            int num1 = 0;
            int num2 = original.Count;
            while (num1 < num2)
            {
                Expression expression1 = (Expression)Visit(original[num1]);
                if (list2 != null)
                {
                    list2.Add(expression1);
                }
                else if (expression1 != original[num1])
                {
                    list2 = new List<Expression>(num2);
                    for (int num3 = 0; num3 < num1; num3++)
                    {
                        list2.Add(original[num3]);
                    }
                    list2.Add(expression1);
                }
                num1++;
            }
            if (list2 != null)
            {
                return list2;
            }
            return original;
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

        internal IEntityType VisitParam(ParameterExpression p)
        {
            Type t = p.Type;
            IEntityType et = TypeSystem.GetEntityType(t);
            return et;
        }

        internal IWhereable VisitParameter(ParameterExpression p)
        {
            Type t = p.Type;
            
            IEntityType et = TypeSystem.GetEntityType(t);
            Console.WriteLine("EntityType: {0}", et.Name);
            
            //IProperty po = et.GetProperty("Name");
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
                    return VisitLambdaExpr((LambdaExpression)exp);
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
                    throw new Exception("not impl");
                    //{
                    //    return this.VisitMemberAccess((MemberExpression)exp);
                    //}
                case ExpressionType.MemberInit:
                    //{
                    //    return this.VisitMemberInit((MemberInitExpression)exp);
                    //}
                case ExpressionType.MethodCall:
                case ExpressionType.MethodCallVirtual:
                    return VisitMethodCallExpr((MethodCallExpression)exp);
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
                    ExceptionThrower(exp);
                    break;
            }
            throw new InvalidOperationException(string.Format("Unhandled Expression Type: {0}", exp.NodeType));
        }

        #region OLDCODE

        //        internal Expression VisitBinary(BinaryExpression b)
//        {
//#if DEBUG
//            Console.WriteLine("VisitBinary");
//            Console.WriteLine("Left: {0}, Right: {1}", b.Left, b.Right);
//#endif

//            // **

//            // **

//            Expression expression3 = this.Visit(b.Left);
//            Expression expression4 = this.Visit(b.Right);
//            if ((expression3 == b.Left) && (expression4 == b.Right))
//            {
//                return b;
//            }
//            return ExpressionVisitor.MakeBinaryExpression(b.NodeType, expression3, expression4);
//        }

//        internal Binding VisitBinding(Binding binding)
//        {
//#if DEBUG
//            Console.WriteLine("VisitBinding\n");
//#endif
//            switch (binding.BindingType)
//            {
//                case BindingType.MemberAssignment:
//                    {
//                        return this.VisitMemberAssignment((MemberAssignment)binding);
//                    }
//                case BindingType.MemberMemberBinding:
//                    {
//                        return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
//                    }
//                case BindingType.MemberListBinding:
//                    {
//                        return this.VisitMemberListBinding((MemberListBinding)binding);
//                    }
//            }
//            throw new InvalidOperationException(string.Format("Unhandled Binding Type: {0}", binding.BindingType));
//        }

//        internal IEnumerable<Binding> VisitBindingList(ReadOnlyCollection<Binding> original)
//        {
//#if DEBUG
//            Console.WriteLine("VisitBindingList\n");
//#endif
//            List<Binding> list2 = null;
//            int num1 = 0;
//            int num2 = original.Count;
//            while (num1 < num2)
//            {
//                Binding binding1 = this.VisitBinding(original[num1]);
//                if (list2 != null)
//                {
//                    list2.Add(binding1);
//                }
//                else if (binding1 != original[num1])
//                {
//                    list2 = new List<Binding>(num2);
//                    for (int num3 = 0; num3 < num1; num3++)
//                    {
//                        list2.Add(original[num3]);
//                    }
//                    list2.Add(binding1);
//                }
//                num1++;
//            }
//            if (list2 != null)
//            {
//                return list2;
//            }
//            return original;
//        }

//        internal Expression VisitConditional(ConditionalExpression c)
//        {
//#if DEBUG
//            Console.WriteLine("VisitConditional\n");
//#endif
//            Expression expression4 = this.Visit(c.Test);
//            Expression expression5 = this.Visit(c.IfTrue);
//            Expression expression6 = this.Visit(c.IfFalse);
//            if (((expression4 == c.Test) && (expression5 == c.IfTrue)) && (expression6 == c.IfFalse))
//            {
//                return c;
//            }
//            return Expression.Condition(expression4, expression5, expression6);
//        }

//        internal Expression VisitConstant(ConstantExpression c)
//        {
//#if DEBUG
//            Console.WriteLine("VisitConstant");
//            Console.WriteLine("Value: {0}, Type: {1}\n", c.Value, c.Type);
//#endif
//            return c;
//        }

//        internal IEnumerable<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
//        {
//            List<Expression> list2 = null;
//            int num1 = 0;
//            int num2 = original.Count;
//            while (num1 < num2)
//            {
//                Expression expression1 = this.Visit(original[num1]);
//                if (list2 != null)
//                {
//                    list2.Add(expression1);
//                }
//                else if (expression1 != original[num1])
//                {
//                    list2 = new List<Expression>(num2);
//                    for (int num3 = 0; num3 < num1; num3++)
//                    {
//                        list2.Add(original[num3]);
//                    }
//                    list2.Add(expression1);
//                }
//                num1++;
//            }
//            if (list2 != null)
//            {
//                return list2;
//            }
//            return original;
//        }

//        internal Expression VisitFunclet(FuncletExpression f)
//        {
//#if DEBUG
//            Console.WriteLine("VisitFunclet");
//#endif
//            return f;
//        }

//        internal Expression VisitInvocation(InvocationExpression iv)
//        {
//#if DEBUG
//            Console.WriteLine("VisitInvocation\n");
//#endif
//            IEnumerable<Expression> enumerable2 = this.VisitExpressionList(iv.Args);
//            Expression expression2 = this.Visit(iv.Expression);
//            if ((enumerable2 != iv.Args) || (expression2 != iv.Expression))
//            {
//                Expression.Invoke((LambdaExpression)expression2, enumerable2);
//            }
//            return iv;
//        }

//        internal Expression VisitLambda(LambdaExpression lambda)
//        {
//            Expression expression2 = this.Visit(lambda.Body);
//            if (expression2 != lambda.Body)
//            {
//                return Expression.Lambda(lambda.Type, expression2, lambda.Parameters);
//            }
//            return lambda;
//        }

//        internal Expression VisitListInit(ListInitExpression init)
//        {
//#if DEBUG
//            Console.WriteLine("VisitListInit\n");
//#endif
//            NewExpression expression2 = this.VisitNew(init.NewExpression);
//            IEnumerable<Expression> enumerable2 = this.VisitExpressionList(init.Expressions);
//            if ((expression2 == init.NewExpression) && (enumerable2 == init.Expressions))
//            {
//                return init;
//            }
//            return Expression.ListInit(expression2, enumerable2);
//        }

        

//        internal MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
//        {
//#if DEBUG
//            Console.WriteLine("VisitMemberAssignment\n");
//#endif
//            Expression expression2 = this.Visit(assignment.Expression);
//            if (expression2 != assignment.Expression)
//            {
//                return Expression.Bind(assignment.Member, expression2);
//            }
//            return assignment;
//        }

//        internal Expression VisitMemberInit(MemberInitExpression init)
//        {
//#if DEBUG
//            Console.WriteLine("VisitMemberInit\n");
//#endif
//            NewExpression expression2 = this.VisitNew(init.NewExpression);
//            IEnumerable<Binding> enumerable2 = this.VisitBindingList(init.Bindings);
//            if ((expression2 == init.NewExpression) && (enumerable2 == init.Bindings))
//            {
//                return init;
//            }
//            return Expression.MemberInit(expression2, enumerable2);
//        }

//        internal MemberListBinding VisitMemberListBinding(MemberListBinding binding)
//        {
//#if DEBUG
//            Console.WriteLine("VisitMemberListBinding");
//#endif
//            IEnumerable<Expression> enumerable2 = this.VisitExpressionList(binding.Expressions);
//            if (enumerable2 != binding.Expressions)
//            {
//                return Expression.ListBind(binding.Member, enumerable2);
//            }
//            return binding;
//        }

//        internal MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
//        {
//#if DEBUG
//            Console.WriteLine("VisitMemberMemberBinding");
//#endif
//            IEnumerable<Binding> enumerable2 = this.VisitBindingList(binding.Bindings);
//            if (enumerable2 != binding.Bindings)
//            {
//                return Expression.MemberBind(binding.Member, enumerable2);
//            }
//            return binding;
//        }

//        internal Expression VisitMethodCall(MethodCallExpression m)
//        {
//            Expression expression2 = this.Visit(m.Object);
//            IEnumerable<Expression> enumerable2 = this.VisitExpressionList(m.Parameters);
//            if ((expression2 == m.Object) && (enumerable2 == m.Parameters))
//            {
//                return m;
//            }
//            return ExpressionVisitor.MakeMethodCallExpression(m.NodeType, expression2, m.Method, enumerable2);
//        }

//        internal NewExpression VisitNew(NewExpression nex)
//        {
//#if DEBUG
//            Console.WriteLine("VisitNew");
//#endif
//            IEnumerable<Expression> enumerable2 = this.VisitExpressionList(nex.Args);
//            if (enumerable2 != nex.Args)
//            {
//                return Expression.New(nex.Constructor, enumerable2);
//            }
//            return nex;
//        }

//        internal Expression VisitNewArray(NewArrayExpression na)
//        {
//#if DEBUG
//            Console.WriteLine("VisitNewArray");
//#endif
//            IEnumerable<Expression> enumerable2 = this.VisitExpressionList(na.Expressions);
//            if (enumerable2 == na.Expressions)
//            {
//                return na;
//            }
//            if (na.NodeType == ExpressionType.NewArrayInit)
//            {
//                return Expression.NewArrayInit(na.Type.GetElementType(), enumerable2);
//            }
//            return Expression.NewArrayBounds(na.Type.GetElementType(), enumerable2);
//        }

        

//        internal Expression VisitTypeIs(TypeBinaryExpression b)
//        {
//#if DEBUG
//            Console.WriteLine("VisitTypeIs\n");
//#endif
//            Expression expression2 = b.Expression;
//            if (expression2 != b.Expression)
//            {
//                return Expression.Is(expression2, b.TypeOperand);
//            }
//            return b;
//        }

//        internal Expression VisitUnary(UnaryExpression u)
//        {
//#if DEBUG
//            Console.WriteLine("VisitUnary");
//#endif
//            Expression expression2 = this.Visit(u.Operand);
//            if (expression2 != u.Operand)
//            {
//                return ExpressionVisitor.MakeUnaryExpression(u.NodeType, expression2, u.Type);
//            }
//            return u;
//        }

        #endregion

        #endregion

    }
}
