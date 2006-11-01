using System;
using System.Collections.Generic;
using System.Text;

namespace AOM
{
    public abstract class AOMException : Exception
    {
        internal AOMException() : base() { /* empty */ }
        internal AOMException(string message) : base(message) { /* empty */ }
    }

    #region exceptions
    public class ValidationException : AOMException
    {
        public ValidationException(string message) : base(message) { }
    }

    public class UnknownPropertyException : AOMException
    {
        string entityTypeName, propertyName;

        private UnknownPropertyException() { /* empty */ }

        public UnknownPropertyException(string entityTypeName, string propertyName)
        {
            this.entityTypeName = entityTypeName;
            this.propertyName = propertyName;
        }

        public override string ToString()
        {
            return "Could not find property " + propertyName + " in entity of type " + entityTypeName
            + System.Environment.NewLine + " (possibly called from sub entity).";
        }
    }

    public class IDChangeAfterCommitException : AOMException { }
    #endregion

}
