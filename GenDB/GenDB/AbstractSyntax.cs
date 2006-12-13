using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    interface IAbsSyntaxVisitor
    {
        void VisitVarProperty(VarProperty vp);
        void VisitCstString(CstString cs);
        void VisitCstBool(CstBool cb);
        void VisitCstLong(CstLong cl);
        void VisitCstDouble(CstDouble cd);
        void VisitCstReference(CstReference cr);
    }

    interface IWhereable
    {
        void AcceptVisitor(IAbsSyntaxVisitor visitor);
    }

    interface IValue { }

    /// <summary>
    /// Contains a reference to a property property.
    /// </summary>
    class VarProperty : IWhereable, IValue
    {
        IProperty property = null;

        internal IProperty Property
        {
            get { return property; }
        }

        public VarProperty(IProperty property)
        {
            this.property = property;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitVarProperty(this);
        }
    }

    class CstString : IWhereable, IValue
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

    class CstBool : IWhereable, IValue
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


    class CstLong : IWhereable, IValue
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

    class CstDouble : IWhereable, IValue
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

    class CstReference : IWhereable, IValue
    {
        IBOReference value;

        public IBOReference Value
        {
            get { return this.value; }
        }

        public CstReference(IBOReference reference)
        {
            this.value = reference;
        }

        public void AcceptVisitor(IAbsSyntaxVisitor visitor)
        {
            visitor.VisitCstReference(this);
        }
    }
}
