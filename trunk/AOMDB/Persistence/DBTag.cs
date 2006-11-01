using System;
using System.Collections.Generic;
using System.Text;

namespace Persistence
{
    /// <summary>
    /// Establish ID and relation to a database.
    /// Has only internal constructors, since this
    /// object is ment ot be modified only by the 
    /// database.
    /// <p>
    /// The Finalize method of this DBTag will
    /// ensure proper removal of the IBusinessObject 
    /// element to which it is associated.
    /// </p>
    /// <p>
    /// About finalization (and other traps for C++ programmers) in C#:
    /// http://www.ondotnet.com/pub/a/oreilly/dotnet/news/programmingCsharp_0801.html
    /// 
    /// When to use destructors and when to use Finalize??????????????????????????????????
    /// 
    /// </p>
    /// </summary>
    public sealed class DBTag
    {
        long id;
        Database owner;

        public long Id
        {
            get { return id; }
            internal set { id = value; }
        }

        private DBTag () { /* empty */ }

        internal DBTag ( Database owner, long id) 
        {
            this.owner = owner;
            this.id = id;
        }
        
        ~DBTag() {
            if (owner != null)
            {
                owner.Delete(this);
                owner = null;
            }
        }
        //public void Finalize() 
        //{
        //    owner.Delete (this);
        //    owner = null;
        //}
    }
}
