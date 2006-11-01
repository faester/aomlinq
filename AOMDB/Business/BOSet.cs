using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Persistence;

namespace Business
{
    public class BOSet : IBOCollection
    {
        private Dictionary<IBusinessObject, bool> dict;
        IBusinessObject[] list = null; //Used when traversed using MoveNext
        IBusinessObject current = null;
        private int position = -1;
        bool isDirty = true;
        DBTag m_tag = null;

        public BOSet()
        {
            dict = new Dictionary<IBusinessObject, bool>();
        }

        public BOSet(IBusinessObject ibo)
            : this()
        {
            Add(ibo);
        }

        public bool IsDirty
        {
            get { return isDirty; }
            set { isDirty = value; }
        }

        public bool Contains(IBusinessObject ibo)
        {
            return dict.ContainsKey(ibo);
        }

        public int Add(IBusinessObject ibo)
        {
            if (!Contains(ibo))
            {
                dict.Add(ibo, false);
                return 1;
            }
            return 0;
        }

        public void Remove(IBusinessObject ibo)
        {
            dict.Remove(ibo);
        }

        public void Clear()
        {
            dict.Clear();
        }

        public int Count
        {
            get { return dict.Count; }
        }

        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }

        public DBTag DatabaseID
        {
            get { return m_tag; }
            set { m_tag = value; }
        }

        public bool MoveNext()
        {
            if (list == null) 
            {
                Reset();
            }
            position++;
            if (position < list.Length )
            {
                current = list[position];
                return true;
            }
            else
            {
                list = null;
                return false;
            }
        }

        public void Reset()
        {
            list = new IBusinessObject[dict.Count];
            dict.Keys.CopyTo(list, 0);
            position = -1;
        }

        public object Current
        {
            get
            {
                return current;
            }

        }
    }
}
