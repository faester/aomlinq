using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB
{
    interface IAbsSyntaxVisitor
    {
        void Visit(IWhereable clause);
        void VisitNumericalProperty(CstProperty vp);
        void VisitCstString(CstString cs);
        void VisitCstBool(CstBool cb);
        void VisitCstLong(CstLong cl);
        void VisitCstDouble(CstDouble cd);
        void VisitCstReference(VarReference cr);
        void VisitPropertyOfReferredObject(PropertyOfReferredObject pro);
        void VisitOPEquals(OP_Equals eq);
        void VisitOPLessThan(OP_LessThan lt);
        void VisitOPGreaterThan(OP_GreaterThan gt);
        void VisitNotExpr(ExprNot expr);
        void VisitAndExpr(ExprAnd expr);
        void VisitOrExpr(ExprOr expr);
        void VisitCstDateTime(CstDateTime cdt);
        void VisitEntityPOIDEquals(EntityPOIDEquals epe);
        void VisitOPNotEquals(OP_NotEquals ieq);
        void VisitInstanceOf(ExprInstanceOf instanceOf);
    }

    interface IWhereable
    {
        void AcceptVisitor(IAbsSyntaxVisitor visitor);
    }

    /// <summary>
    /// An operator such as equals, less than etc.
    /// </summary>
    interface IBoolOperator : IExpression {}

    /// <summary>
    /// An expression. 
    /// </summary>
    interface IExpression : IWhereable { }

    /// <summary>
    /// Something with a value
    /// </summary>
    interface IValue : IWhereable { } 

    /// <summary>
    /// Constant values.
    /// </summary>
    interface IConstant : IValue    {}

    /// <summary>
    /// Numerical values. Can be compared using LE, GT etc.
    /// </summary>
    interface INumerical : IConstant {}

    /// <summary>
    /// String values. More restricted comparison options than numerical.
    /// </summary>
    interface IStringvalue : IConstant {}

    class ExprNot : IExpression
    {
        IExpression expression;

        internal IExpression Expression
        {
            get { return expression; }
        }

        public ExprNot(IExpression expr)
        {
            this.expression = expr;
        }

        public void AcceptVisitor (IAbsSyntaxVisitor visitor)
        {
            visitor.VisitNotExpr(this);
        }
    }

    class ExprInstanceOf : IExpression
    {
        Type clrType;

        public Type ClrType
        {
            get { return clrType; }
        }

        public ExprInstanceOf(Type clrType)
        {
            this.clrType = clrType;
        }

        public void AcceptVisitor (IAbsSyntaxVisitor visitor)
        {
            visitor.VisitInstanceOf (this);
        }
    }

    abstract class AbsBinaryExpression : IExpression
    {
        IExpression left;

        internal IExpression Left
        {
            get { return left; }
            set { left = value; }
        }
        IExpression right;

        internal IExpression Right
        {
            get { return right; }
            set { right = value; }
        }

        public AbsBinaryExpression(IExpression left, IExpression right)
        {
            this.left = left;
            this.right = right;
        }

        public abstract void AcceptVisitor(IAbsSyntaxVisitor visitor);
    }

    class ExprAnd : AbsBinaryExpression
    {
        public ExprAnd(IExpression l, IExpression r) : base(l, r) { /* empty */ }

        public override void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitAndExpr (this);
        }
    }

    class ExprOr : AbsBinaryExpression
    {
        public ExprOr(IExpression l, IExpression r) : base (l, r) { /* empty */ }

        public override void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitOrExpr(this);
        }
    }

    abstract class BinaryOperator : IBoolOperator
    {
        IValue left;
        internal IValue Left
        {
            get { return left; }
        }

        IValue right;
        internal IValue Right
        {
            get { return right; }
        }

        public BinaryOperator (IValue left, IValue right)
        {
            this.left = left;
            this.right = right;
        }

        public abstract void AcceptVisitor(IAbsSyntaxVisitor visitor);
    }

    class OP_Equals :  BinaryOperator, IBoolOperator
    {
        public OP_Equals(IValue left, IValue right) : base(left, right) { }

        public override void AcceptVisitor (IAbsSyntaxVisitor visitor)
        {
            visitor.VisitOPEquals(this);
        }
    }

    class OP_LessThan : BinaryOperator, IBoolOperator
    {
        public OP_LessThan(IValue left, IValue right) : base(left, right) { }
        public override void AcceptVisitor (IAbsSyntaxVisitor visitor)
        {
            visitor.VisitOPLessThan(this);
        }
    }

    class OP_GreaterThan : BinaryOperator, IBoolOperator
    {
        public OP_GreaterThan(IValue left, IValue right) : base(left, right) { }
        public override void AcceptVisitor (IAbsSyntaxVisitor visitor)
        {
            visitor.VisitOPGreaterThan(this);
        }
    }

    class OP_NotEquals : BinaryOperator, IBoolOperator
    {
        public OP_NotEquals(IValue left, IValue right) : base (left, right) {}
        public override void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitOPNotEquals(this);
        }
    }

    /// <summary>
    /// Contains a reference to a property property.
    /// 
    /// Find property: TypeSystem.GetEntityType(obj.GetType()).GetProperty
    /// </summary>
    class CstProperty : INumerical
    {
        IProperty property = null;

        internal IProperty Property
        {
            get { return property; }
        }

        public CstProperty(IProperty property)
        {
            this.property = property;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitNumericalProperty(this);
        }
    }

    class CstDateTime : INumerical
    {
        DateTime value;

        public DateTime Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public CstDateTime(DateTime value)
        {
            this.value = value;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitCstDateTime(this);
        }
    }

    class CstString : IStringvalue
    {
        string value;

        public string Value
        {
            get { return this.value; }
        }

        public CstString(string value)
        {
            this.value = value;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitCstString(this);
        }
    }

    class CstBool : IConstant
    {
        bool value;

        public CstBool (bool value)
        {
            this.value = value;
        }

        public bool Value
        {
            get { return this.value; }
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitCstBool(this);
        }
    }


    class CstLong : INumerical
    {
        long value;

        public CstLong(long value)
        {
            this.value = value;
        }

        public long Value
        {
            get { return this.value; }
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitCstLong(this);
        }
    }

    class CstDouble : INumerical
    {
        double value;

        public CstDouble (double value)
        {
            this.value = value;
        }

        public double Value
        {
            get { return this.value; }
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitCstDouble(this);
        }
    }

    /// <summary>
    /// A reference to an object.
    /// </summary>
    class VarReference : IValue 
    {
        IBOReference value;

        public IBOReference Value
        {
            get { return this.value; }
        }

        public VarReference(IBOReference reference)
        {
            this.value = reference;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitCstReference(this);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class PropertyOfReferredObject : IWhereable, IValue
    {
        VarReference referredObject;

        internal VarReference ReferredObject
        {
            get { return referredObject; }
            set { referredObject = value; }
        }

        IConstant referencedField;

        internal IConstant ReferencedField
        {
            get { return referencedField; }
            set { referencedField = value; }
        }

        public PropertyOfReferredObject (VarReference referredObject, IConstant referencedField)
        {
            if (referredObject == null) { throw new NullReferenceException("referredObject"); }
            if (referredObject == null) { throw new NullReferenceException("referredObject"); }
            this.referencedField = referencedField;
            this.referredObject = referredObject;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitPropertyOfReferredObject(this);
        }
    }

    class EntityPOIDEquals : IExpression
    {
        long entityPOID;

        public long EntityPOID
        {
            get { return entityPOID; }
        }

        public EntityPOIDEquals(long entityPOID)
        {
            this.entityPOID = entityPOID;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitEntityPOIDEquals(this);
        }
    }
}
