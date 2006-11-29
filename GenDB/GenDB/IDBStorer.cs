using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    interface IDBStorer
    {
        void Store(GenericDB storage, Object o);
        object Retrieve(GenericDB storage);
    }
}
