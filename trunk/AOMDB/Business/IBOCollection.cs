using System;
using System.Text;
using System.Collections;

namespace Business
{
    public interface IBOCollection : IEnumerable, IEnumerator, IBusinessObject
    {
        int Add(IBusinessObject ibo);
        void Remove(IBusinessObject ibo);
        bool Contains(IBusinessObject ibo);
        void Clear();
        int Count {get;}
    }

    public interface IBOOrderedCollection : IBOCollection
    {
        void Insert(int index, IBusinessObject ibo);
    }
}
