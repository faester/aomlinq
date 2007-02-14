using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Query;
using GenDB.DB;
using System.Expressions;

namespace GenDB
{
    internal class SqlJoinTranslator
    {
        TypeSystem typeSystem;
        bool leftSideDone;
        bool isOuterJoin;
        List<IProperty> outerProps = new List<IProperty>();
        List<IProperty> innerProps = new List<IProperty>();

        internal SqlJoinTranslator(TypeSystem typeSystem)
        {
            this.typeSystem = typeSystem;
        }

        public IExpression Convert(Expression outer, Expression inner, bool isOuterJoin)
        {
            this.isOuterJoin=isOuterJoin;
            VisitExpr(outer);
            VisitExpr(inner);
            throw new Exception("stop");
        }

        internal void VisitExpr(Expression expr)
        {
            if(expr.NodeType.ToString() == "Lambda")
            {
                LambdaExpression lambda = (LambdaExpression)expr;
                if(lambda.Body.NodeType.ToString()=="MemberAccess")
                {
                    VisitMemberExpression(lambda);
                }
                else if(lambda.Body.NodeType.ToString()=="MemberInit")
                {
                    VisitMemberInit(lambda);
                }
                else
                {
                    throw new Exception("Unknown NodeType: "+lambda.Body.NodeType.ToString());
                }
            }
            else
            {
                throw new Exception("Unknown NodeType: "+expr.NodeType.ToString());
            }
            
        }
        
        internal void VisitMemberExpression(LambdaExpression lambda)
        {
            MemberExpression me = (MemberExpression)lambda.Body;
            Type type = me.Expression.Type;
            if(!typeSystem.IsTypeKnown(type))
            {
                    typeSystem.RegisterType(type);
            }
            IEntityType et = typeSystem.GetEntityType(type);
            IProperty prop = et.GetProperty(me.Member.Name);
            if(!leftSideDone)
            {   
                outerProps.Add(prop);
            } 
            else
            {
                innerProps.Add(prop);
            }
            leftSideDone=!leftSideDone;
        }

        internal void VisitMemberInit(LambdaExpression lambda)
        {
            MemberInitExpression mie = (MemberInitExpression) lambda.Body;
            Type type = lambda.Parameters[0].Type;
            if(!typeSystem.IsTypeKnown(type))
            {
                typeSystem.RegisterType(type);
            }
            IEntityType et = typeSystem.GetEntityType(type);
            for(int i=0;i<mie.Bindings.Count;i++)
            {
                IProperty prop = et.GetProperty(mie.Bindings[i].Member.Name);
                if(!leftSideDone)outerProps.Add(prop);
                else innerProps.Add(prop);
            }
            leftSideDone=!leftSideDone;
        }
    }
}
