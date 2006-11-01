using System;
using System.Collections.Generic;
using System.Text;
using Business;
using Persistence;

namespace Tests
{
    /// <summary>
    /// Used for testing purposes. Should contain all
    /// data s_types that the conversion mechanism should 
    /// allow.
    /// </summary>
    internal class SimpleBusinessObject : IBusinessObject
    {
        enum SBOEnum { Per, Poul, Konrad }
        DBTag m_tag;

        byte byteMember = 123;

        public byte ByteMember
        {
            get { return byteMember; }
            set { byteMember = value; }
        }

        char charMember = 'a';

        DateTime datetimeMember = DateTime.Now;

        string firstEqualsFailFieldname = null;

        /// <summary>
        /// Contains the member field that
        /// made last equals call return false.
        /// If last equals call returned true
        /// or equals has not yet been called on
        /// this object, FirstEqualsFailFieldname
        /// will be null.
        /// </summary>
        public string FirstEqualsFailFieldname
        {
            get { return firstEqualsFailFieldname; }
        }

        SBOEnum enumMember = SBOEnum.Konrad;

        long longMember = long.MinValue;

        public long LongMember
        {
            get { return longMember; }
        }

        private SBOEnum EnumMember
        {
            get { return enumMember; }
            set { enumMember = value; }
        }


        public DateTime DatetimeMember
        {
            get { return datetimeMember; }
            set { datetimeMember = value; }
        }

        float floatMember = default(float) + 10;

        public float FloatMember
        {
            get { return floatMember; }
            set { floatMember = value; }
        }

        double doubleMember = default(double) + 10;

        public double DoubleMember
        {
            get { return doubleMember; }
            set { doubleMember = value; }
        }
        bool boolMember = !default(bool);

        public bool BoolMember
        {
            get { return boolMember; }
            set { boolMember = value; }
        }

        public DBTag DatabaseID
        {
            get { return m_tag; }
            set { m_tag = value; }
        }

        public bool IsDirty
        {
            get { return true; }
            set { }
        }

        private string name = "";

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private int serial = 0;

        public int Serial
        {
            get { return serial; }
            set { serial = value; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is SimpleBusinessObject))
            {
                return false;
            }
            if (object.ReferenceEquals(obj, this))
            {
                return true;
            }
            SimpleBusinessObject other = (SimpleBusinessObject)obj;
            firstEqualsFailFieldname = null;
            /* Statements below is used to be able to report 
             * which fields caused inequality.
             */
            if (!
                ((other.m_tag == null && this.m_tag == null) || 
                (this.m_tag == null && this.m_tag.Equals(other.m_tag)))
                )
            {
                firstEqualsFailFieldname = "m_tag";
                return false;
            }
            if (this.name != other.name)
            {
                firstEqualsFailFieldname = "name";
                return false;
            }
            if (this.serial != other.serial)
            {
                firstEqualsFailFieldname = "serial";
                return false;
            }
            if (this.boolMember != other.boolMember)
            {
                firstEqualsFailFieldname = "boolMember";
                return false;
            }
            if (this.floatMember != other.floatMember)
            {
                firstEqualsFailFieldname = "floatMember";
                return false;
            }
            if (this.doubleMember != other.doubleMember)
            {
                firstEqualsFailFieldname = "doubleMember";
                return false;
            }
            if (this.datetimeMember != other.datetimeMember)
            {
                firstEqualsFailFieldname = "datetimeMember";
                return false;
            }
            if (this.enumMember != other.enumMember)
            {
                firstEqualsFailFieldname = "enumMember";
                return false;
            }
            if (this.longMember != other.longMember )
            {
                firstEqualsFailFieldname = "longMember";
                return false;
            }
            if (this.charMember != other.charMember )
            {
                firstEqualsFailFieldname = "charMember";
                return false;
            }
            if (this.byteMember != other.byteMember )
            {
                firstEqualsFailFieldname = "byteMember";
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
