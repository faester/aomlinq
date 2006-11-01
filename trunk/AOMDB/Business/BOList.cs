using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Persistence;

namespace Business
{
    public class BOList : IBOCollection
    {
        IList list = new List<IBusinessObject>();
        private int position = -1;
        bool isDirty = true;
        DBTag m_tag = null;

        public bool IsDirty
        {
            get { return isDirty; }
            set { isDirty = value; }
        }
        
        #region IEnumerable methods

        public DBTag DatabaseID
        {
            get {return m_tag;}
            set {m_tag = value;}
        }

        public bool MoveNext()
        {
            position++;
            return position < list.Count;
        }

        public void Reset()
        {
            position = -1;
        }

        public object Current
        {
            get
            {
                return list[position];
            }
        }

        #endregion IEnumerable methods

        #region IEnumerable methods

        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }

        #endregion IEnumerable methods

        #region IBOCollection methods

        public int Add(IBusinessObject ibo)
        {
            return list.Add(ibo);
        }

        public void Remove(IBusinessObject ibo)
        {
            list.Remove(ibo);
        }

        public void Clear()
        {
            list.Clear();
        }

        public int Count
        {
            get { return list.Count; }
        }

        #endregion IBOCollection methods

        #region BOList Collection methods

        public void Insert(int index, IBusinessObject ibo)
        {
            list.Insert(index, ibo);
        }

        public bool Contains(IBusinessObject ibo)
        {
            return list.Contains(ibo);
        }

        public void AddFirst(IBusinessObject ibo) 
        {
            IBusinessObject[] iboArr = new IBusinessObject[list.Count+1];
            list.CopyTo(iboArr, 1);
            iboArr[0]= ibo;
            list = new List<IBusinessObject>();
            for(int i=0; i<iboArr.Length; i++)
            {
                list.Add(iboArr[i]);   
            }
        }
        #endregion BOList Collection methods
    }
}
