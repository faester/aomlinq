using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

//[Serializable]
//[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.Native)]
public struct PropertyType : INullable
{
    private bool m_Null;
    private long _propertyTypePOID;
    private string _name;

    public PropertyType(long propertyTypePOID, string name)
    {
        _propertyTypePOID = propertyTypePOID;
        _name = name;
        m_Null = false;
    }

    public long PropertyTypePOID
    {
        get{return _propertyTypePOID;}
        set{_propertyTypePOID=value;}
    }

    public string Name
    {
        get{return _name;}
        set{_name=value;}
    }

    public override string ToString()
    {
        return "PropertyTypePOID: "+_propertyTypePOID+", Name: "+_name;
    }

    public bool IsNull
    {
        get
        {
            return m_Null;
        }
    }

    public static PropertyType Null
    {
        get
        {
            PropertyType h = new PropertyType();
            h.m_Null = true;
            return h;
        }
    }

    public static PropertyType Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;
        PropertyType u = new PropertyType();
        
        return u;
    }
}


