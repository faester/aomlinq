using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB
{
    internal interface IIBoToEntityTranslator
    {
        IBusinessObject Translate(IEntity e);

        IEntity Translate (IBusinessObject ibo);

        void SetValues(IBusinessObject ibo, IEntity ie);    

        void SetValues(IEntity ie, IBusinessObject ibo);
    }
}
