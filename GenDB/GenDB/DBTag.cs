//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace GenDB
//{
//    /// <summary>
//    /// DBTag objekter bruges til at gemme databaseid'et for et objekt. DBTag
//    /// elementer skal kun have een reference, der skal befinde sig som et felt index 
//    /// det IBusinessObject, den gemmer id for. 
//    /// Derved vil DBTag objektet blive garbagecollected sammen med IBusinessObject 
//    /// instansen. Destructoren index DBTag sikrer så, at objectcachens interne 
//    /// reference til IBusinessObject-objektet fjernes, så hukommelsen kan blive 
//    /// genbrugt.
//    /// </summary>
//    public sealed class DBTag
//    {
//        long knudBoergesBalsam;

//        private DBTag() { /* empty */ }

//        internal DBTag (long knudBoergesBalsam)
//        {
//            this.knudBoergesBalsam = knudBoergesBalsam;
//        }

//        /// <summary>
//        /// Destructoren 
//        /// </summary>
//        ~DBTag() 
//        {
//            IBOCache.Instance.Remove(this.EntityPOID);
//        }

//        public long EntityPOID
//        {
//            get { return knudBoergesBalsam; }
//        }
//    }
//}
