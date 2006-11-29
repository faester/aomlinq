using System;
using System.Collections.Generic;
using System.Text;

namespace DBLayer
{
    internal interface ICondition
    {
        string SqlConversion();
        void AppendTo(StringBuilder sb);
    }  

    internal class BinaryCondition : ICondition
    {
        ICondition left, right;
        string oper;

        protected BinaryCondition() { /* empty */} 
        protected BinaryCondition(ICondition left, string oper, ICondition right)
        {
            this.left = left;
            this.right = right;
            this.oper = oper;
        }

        public void AppendTo(StringBuilder sb)
        {
            sb.Append ('(');
            left.AppendTo (sb);
            sb.Append (' ');
            sb.Append(oper);
            sb.Append (' ');
            right.AppendTo (sb);
            sb.Append (')');
        }

        public string SqlConversion() 
        {
            StringBuilder sb = new StringBuilder();
            AppendTo(sb);
            return sb.ToString();
        }
    }

    internal class SqlAnd : BinaryCondition{
        private SqlAnd () : base() { /* empty */ }
        public SqlAnd(ICondition left, ICondition right)
            : base (left, "AND", right)
        { /* empty */ }
    }

    internal class SqlOr : BinaryCondition
    {
        private SqlOr () : base() { /* empty */ }
        public SqlOr(ICondition left, ICondition right)
            : base (left, "OR", right)
        { /* empty */ }
    }

    internal class SqlNot : ICondition 
    {
        ICondition cond;
        protected SqlNot() { /* empty */ }
        public SqlNot(ICondition cond)
        {
            this.cond = cond;
        }

        public void AppendTo(StringBuilder sb)
        {
            sb.Append (" NOT (");
            cond.AppendTo (sb);
            sb.Append (')');
        }

        public string SqlConversion()
        {
            StringBuilder sb = new StringBuilder();
            AppendTo(sb);
            return sb.ToString();
        }
    }   
}
