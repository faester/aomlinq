using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB.DB
{
    internal class PropertyValue : IPropertyValue
    {
        private IProperty property;
        private IEntity entity;
        private string stringValue = null;
        private DateTime dateTimeValue = default(DateTime);
        bool existsInDatabase = false;
        private long longValue = default(long);
        private bool boolValue = false;
        private char charValue = default(char);
        private IBOReference refValue = new IBOReference(true, 0);
        private double doubleValue = default(double);

        public PropertyValue (IProperty property, IEntity entity)
        {
            this.entity = entity;
            this.property = property;
        }

        public double DoubleValue
        {
            get { return doubleValue; }
            set { doubleValue = value; }
        }

        public IBOReference RefValue
        {
            get { return refValue; }
            set { refValue = value; }
        }

        public char CharValue
        {
            get { return charValue; }
            set { charValue = value; }
        }


        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        public long LongValue
        {
            get { return longValue; }
            set { longValue = value; }
        }

        public bool ExistsInDatabase
        {
            get { return existsInDatabase; }
            set { existsInDatabase = value; }
        }

        public DateTime DateTimeValue
        {
            get { return dateTimeValue; }
            set { dateTimeValue = value; }
        }

        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }

        public IEntity Entity
        {
            get { return entity; }
        }

        public IProperty Property
        {
            get { return property; }
        }

        public override string ToString()
        {
            string value = null;
            switch (property.MappingType)
            {
                case MappingType.BOOL: value = BoolValue.ToString(); break;
                case MappingType.DATETIME: value = DateTimeValue.ToString(); break;
                case MappingType.DOUBLE: value = DoubleValue.ToString(); break;
                case MappingType.LONG: value = LongValue.ToString(); break;
                case MappingType.REFERENCE: value = this.RefValue.ToString(); break;
                case MappingType.STRING: value = this.StringValue; break;
            }
            return property.MappingType.ToString() + " = " + value;

        }
    }
}
