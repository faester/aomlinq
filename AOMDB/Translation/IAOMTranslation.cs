using System;
using System.Collections.Generic;
using System.Text;
using AOM;
using Business;

namespace Translation
{
    public interface IAOMConverter <T>  
        where T : IBusinessObject, new()
    {
        Entity ToEntity(T obj);
        T FromEntity(Entity e);
    }
}
