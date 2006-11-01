using System;
using System.Text;

namespace AOM
{
    /// <summary>
    /// Convenience class used to make 
    /// all the AOM-classes comparable based
    /// on their names. 
    /// </summary>
    public abstract class AOMBaseObject : IComparable<AOMBaseObject>
    {
        /// <summary>
        /// Specifies that a key does not correspond to a key in the 
        /// db. (The object has never been comitted to the db)
        /// </summary>
        public const long UNDEFINED_ID = -1;

        string name;
        long id = UNDEFINED_ID;
        bool isPersistent = false;

        /// <summary>
        /// Used to indicate that the object already
        /// exists in the database. 
        /// </summary>
        public bool IsPersistent
        {
            get { return isPersistent; }
            set { isPersistent = value; }
        }


        /// <summary>
        /// Id must be set when data are stored in the 
        /// database or retrieved from the database. 
        /// </summary>
        public virtual long Id
        {
            get { return id; }
            set
            {
                if (isPersistent) { throw new IDChangeAfterCommitException(); }
                id = value;
            }
        }

        /// <summary>
        /// If AOMBaseObject has been written to 
        /// persistent storage it has had its ID 
        /// set to some unique value, and this 
        /// value is used as hash code. 
        /// <p>
        /// If 'this' is not persistent, the bases'
        /// hash code will be used. 
        /// </p> 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (IsPersistent) {
                return (int)(Id % int.MaxValue);
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public bool HasUndefinedId
        {
            get { return id == UNDEFINED_ID; }
        }

        public string Name
        {
            get { return name; }
            internal set { name = value; }
        }

        int IComparable<AOMBaseObject>.CompareTo(AOMBaseObject other)
        {
            return name.CompareTo(other.name);
        }
    }

    /// <summary>
    /// TODO: Remove!
    /// </summary>
    public static class AOMConfig
    {
        public const string CNN_STRING = "server=.;database=tests;uid=aom;pwd=aomuser";
        //private static Persistence.IStorage storage = Persistence.DataBase.Instance;

        //internal static Persistence.IStorage Storage
        //{
        //    get { return AOMConfig.storage; }
        //    private set { AOMConfig.storage = value; }
        //}
    }
}
