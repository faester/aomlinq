using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB.AbstractSyntax
{
    /// <summary>
    /// Interface for en visitor, der kan behandle et
    /// udtryk i abstrakt syntaks. 
    /// </summary>
    interface IAbsSyntaxVisitor
    {
        void Visit(IWhereable clause);
        void VisitCstThis(CstThis cstThis);
        void VisitProperty(CstProperty vp);
        void VisitCstString(CstString cs);
        void VisitCstBool(CstBool cb);
        void VisitCstLong(CstLong cl);
        void VisitCstChar(CstChar ch);
        void VisitCstDouble(CstDouble cd);
        void VisitCstReference(VarReference cr);
        void VisitNestedReference(NestedReference pro);
        void VisitOPEquals(BoolEquals eq);
        void VisitOPLessThan(BoolLessThan lt);
        void VisitOPGreaterThan(BoolGreaterThan gt);
        void VisitNotExpr(ExprNot expr);
        void VisitAndExpr(ExprAnd expr);
        void VisitOrExpr(ExprOr expr);
        void VisitCstDateTime(CstDateTime cdt);
        //void VisitEntityPOIDEquals(EntityPOIDEquals epe);
        void VisitOPNotEquals(OP_NotEquals ieq);
        void VisitInstanceOf(ExprInstanceOf instanceOf);
        void VisitExprIsTrue(ExprIsTrue valueIsTrue);
        void VisitExprIsFalse(ExprIsFalse valueIsFalse);
        void VisitNotSqlTranslatable(ExprNotTranslatable nts);
        void VisitValNotTranslatable(ValNotTranslatable cst);
        void VisitArithmeticOperator(ArithmeticOperator oper);
    }

    /// <summary>
    /// Roden af hierarkiet i den abstrakte syntaks. 
    /// Definerer kun selve visit-metoden, der benyttes 
    /// i forbindelse med double-dispatch.
    /// </summary>
    interface IWhereable
    {
        void AcceptVisitor(IAbsSyntaxVisitor visitor);
    }

    /// <summary>
    /// Beskriver en boolsk operator. 
    /// </summary>
    interface IBoolOperator : IExpression { }

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
    interface IConstant : IValue { }

    /// <summary>
    /// Numerical values. Can be compared using LE, GT etc.
    /// </summary>
    interface INumerical : IConstant { }

    /// <summary>
    /// String values. More restricted comparison options than numerical.
    /// </summary>
    interface IStringvalue : IConstant { }

    #region ArithmeticOperators
    abstract class ArithmeticOperator : IValue
    {
        static bool IsString(IValue val)
        {
            return val is CstString || (val is CstProperty && (val as CstProperty).Property.MappingType == MappingType.STRING);
        }

        IValue left, right;
        bool leftIsString, rightIsString;

        public bool RightIsString
        {
            get { return rightIsString; }
        }

        public bool LeftIsString
        {
            get { return leftIsString; }
        }

        public ArithmeticOperator(IValue left, IValue right)
        {
            this.left = left;
            this.right = right;
            rightIsString = ArithmeticOperator.IsString (right);
            leftIsString = ArithmeticOperator.IsString (left);
        }

        public IValue Left
        {
            get { return left; }
        }

        public IValue Right
        {
            get { return right; }
        }

        public abstract string OperatorSymbol
        {
            get;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitArithmeticOperator(this);
        }
    }

    class OPMult : ArithmeticOperator
    {
        public OPMult(IValue left, IValue right)
            : base(left, right)
        { }

        public override string OperatorSymbol
        {
            get { return "*"; }
        }
    }

    class OPDiv : ArithmeticOperator
    {
        public OPDiv(IValue left, IValue right)
            : base(left, right)
        { }

        public override string OperatorSymbol
        {
            get { return "/"; }
        }
    }


    class OPPlus : ArithmeticOperator
    {
        public OPPlus(IValue left, IValue right)
            : base(left, right)
        { }

        public override string OperatorSymbol
        {
            get { return "+"; }
        }
    }

    class OPMinus : ArithmeticOperator
    {
        public OPMinus(IValue left, IValue right)
            : base(left, right)
        { }
        public override string OperatorSymbol
        {
            get { return "-"; }
        }
    }
    #endregion

    class CstThis : IConstant
    {
        private CstThis() { }
        private static CstThis instance = new CstThis();

        internal static CstThis Instance
        {
            get { return CstThis.instance; }
            set { CstThis.instance = value; }
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitCstThis(this);
        }

        public override string ToString()
        {
            return "CstThis";
        }
    }

    /// <summary>
    /// Benyttes til at indikere, at en knude i et Expression-træ ikke 
    /// kan oversættes til SQL, eller i hvert fald at oversættelsen ikke
    /// er understøttet af frameworket. 
    /// 
    /// SqlExpressionTranslator-objekter indsætter et element af denne 
    /// type, i den abstrakte syntaks, der produceres når et Expression-træ
    /// parses, hvorved det bliver muligt at adskille de dele af det oprindelige
    /// udtryk, der kan oversættes til SQL, og samtidig afgøre, om det efterfølgende
    /// er nødvendigt at teste resultatsættet fra databasen mod den oprindelige 
    /// Expression, i form af dens kompilerede Func &gt; T, boolean &lt; -udtryk.
    /// </summary>
    class ExprNotTranslatable : IExpression
    {
        private ExprNotTranslatable() { }

        private static ExprNotTranslatable instance = new ExprNotTranslatable();

        internal static ExprNotTranslatable Instance
        {
            get { return instance; }
        }



        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitNotSqlTranslatable(this);
        }

        public override string ToString()
        {
            return "CstNotTranslatable";
        }
    }



    class ExprIsTrue : IExpression
    {
        private ExprIsTrue() { /* empty, singleton */ }

        static ExprIsTrue instance = new ExprIsTrue();

        internal static ExprIsTrue Instance
        {
            get { return ExprIsTrue.instance; }
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitExprIsTrue(this);
        }

        public override string ToString()
        {
            return "CstIsTrue";
        }
    }

    class ValNotTranslatable : IValue
    {
        private ValNotTranslatable() { /* empty */ }
        private static ValNotTranslatable instance = new ValNotTranslatable();

        internal static ValNotTranslatable Instance
        {
            get { return ValNotTranslatable.instance; }
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitValNotTranslatable(this);
        }

    }

    class ExprIsFalse : IExpression
    {
        private ExprIsFalse() { /* empty, singleton */ }

        static ExprIsFalse instance = new ExprIsFalse();

        internal static ExprIsFalse Instance
        {
            get { return ExprIsFalse.instance; }
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitExprIsFalse(this);
        }

        public override string ToString()
        {
            return "CstIsFalse";
        }
    }

    class ExprNot : IExpression
    {
        IExpression expression;

        internal IExpression Expression
        {
            get { return expression; }
            set { expression = value; }
        }

        public ExprNot(IExpression expr)
        {
            this.expression = expr;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitNotExpr(this);
        }

        public override string ToString()
        {
            return "ExprNot(" + expression + ")";
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

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitInstanceOf(this);
        }
        public override string ToString()
        {
            return "ExprInstanceOf (" + clrType + ")";
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
            visitor.VisitAndExpr(this);
        }

        public override string ToString()
        {
            return "ExprAnd (" + Left + "," + Right + ")";
        }
    }

    class ExprOr : AbsBinaryExpression
    {
        public ExprOr(IExpression l, IExpression r) : base(l, r) { /* empty */ }

        public override void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitOrExpr(this);
        }
        public override string ToString()
        {
            return "ExprOr (" + Left + "," + Right + ")";
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

        public BinaryOperator(IValue left, IValue right)
        {
            this.left = left;
            this.right = right;
        }

        public abstract void AcceptVisitor(IAbsSyntaxVisitor visitor);
    }

    class BoolEquals : BinaryOperator, IBoolOperator
    {
        public BoolEquals(IValue left, IValue right) : base(left, right) { }

        public override void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitOPEquals(this);
        }

        public override string ToString()
        {
            return "OP_Equals(" + Left.ToString() + "," + Right.ToString() + ")";
        }
    }

    class BoolLessThan : BinaryOperator, IBoolOperator
    {
        public BoolLessThan(IValue left, IValue right) : base(left, right) { }
        public override void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitOPLessThan(this);
        }
        public override string ToString()
        {
            return "OP_LessThan(" + Left.ToString() + "," + Right.ToString() + ")";
        }
    }

    class BoolGreaterThan : BinaryOperator, IBoolOperator
    {
        public BoolGreaterThan(IValue left, IValue right) : base(left, right) { }
        public override void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitOPGreaterThan(this);
        }
        public override string ToString()
        {
            return "OP_GreaterThan(" + Left.ToString() + "," + Right.ToString() + ")";
        }

    }

    class OP_NotEquals : BinaryOperator, IBoolOperator
    {
        public OP_NotEquals(IValue left, IValue right) : base(left, right) { }
        public override void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitOPNotEquals(this);
        }
        public override string ToString()
        {
            return "OP_NotEquals(" + Left.ToString() + "," + Right.ToString() + ")";
        }

    }

    /// <summary>
    /// Contains a reference to a cstProperty cstProperty.
    /// 
    /// Find cstProperty: TypeSystem.GetEntityType(obj.GetType()).GetProperty
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
            visitor.VisitProperty(this);
        }

        public override string ToString()
        {
            return "CstProperty(" + (property != null ? property.ToString() : "null") + ")";
        }
    }

    class CstDateTime : IConstant
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

        public override string ToString()
        {
            return "CstDateTime (" + this.Value + ")";
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


        public override string ToString()
        {
            return "CstString (" + this.Value + ")";
        }
    }

    class CstChar : IStringvalue
    {
        char ch;

        public char Ch
        {
            get { return ch; }
        }

        public CstChar(char ch)
        {
            this.ch = ch;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitCstChar(this);
        }

        public override string ToString()
        {
            return "CstChar (" + this.ch + ")";
        }
    }

    class CstBool : IConstant
    {
        bool value;

        public CstBool(bool value)
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

        public override string ToString()
        {
            return "CstBool (" + value + ")";
        }
    }


    class CstLong : IConstant
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
        public override string ToString()
        {
            return "CstLong (" + value + ")";
        }
    }

    class CstDouble : INumerical
    {
        double value;

        public CstDouble(double value)
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
        public override string ToString()
        {
            return "CstDouble (" + value + ")";
        }
    }

    /// <summary>
    /// A reference to an object.
    /// </summary>
    class VarReference : IValue
    {
        /// <summary>
        /// Stores the object to test internally, since it may
        /// be persisted after the expression is created.
        /// </summary>
        WeakReference reference = null;
        //IBusinessObject referencedObject = null;

        public IBOReference Value
        {
            get
            {
                if (reference == null) { return new IBOReference(true); }
                IBusinessObject referencedObject = !reference.IsAlive ? null : (IBusinessObject)reference.Target;
                if (referencedObject == null
                    || !referencedObject.DBIdentity.IsPersistent)
                {
                    return new IBOReference(true);
                }
                else
                {
                    return new IBOReference(referencedObject.DBIdentity);
                }
            }
        }

        public Type TypeOfReference
        {
            get { return reference.Target.GetType(); }
        }

        public VarReference(IBusinessObject referencedObject)
        {
            if (referencedObject != null)
            {
                this.reference = new WeakReference(referencedObject);
            }
            //this.referencedObject = referencedObject;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitCstReference(this);
        }

        public override string ToString()
        {
            if (this.reference != null)
            {
                return "VarReference(" + (this.reference.IsAlive ? reference.Target : "GC'ed") + ")";
            }
            else
            {
                return "VarReference(null)";
            }
        }
    }

    /// <summary>
    /// Benyttes når en betingelse indeholder værdier af referencefelters
    /// properties. Eksempelvis Person.Spouse.Father.Name
    /// 
    /// Dette skal omskrives til en NestedReferenced(new NestedReference(new NestedReference(new NestedReference(null, propertyPOIDofName), propertyPoidOfFather), propertyPOIDOfSpouse)))
    /// </summary>
    class NestedReference : IValue
    {
        NestedReference innerReference;
        CstProperty cstProperty;

        internal CstProperty CstProperty
        {
            get { return cstProperty; }
        }

        internal NestedReference InnerReference
        {
            get { return innerReference; }
        }

        public NestedReference(NestedReference inner, CstProperty property)
        {
            innerReference = inner;
            this.cstProperty = property;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitNestedReference(this);
        }

        public override string ToString()
        {
            return "NestedReference(" + CstProperty + ", inner = " + (innerReference == null ? "null" : innerReference.ToString()) + ")";
        }
    }
}
