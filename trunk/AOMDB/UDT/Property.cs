using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

//[Serializable]
//[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.Native)]
public struct Property : INullable
{
    private bool m_Null;
    private long _propertyPOID;
    private string _name;
    private long _propertyTypePOID;
    private long _entityTypePOID;
    private string _defaultValue;

    public Property(long propertyPOID,
                    string name,
                    long propertyTypePOID,
                    long entityTypePOID,
                    string defaultValue)
    {
        _propertyPOID = propertyPOID;
        _name = name;
        _propertyTypePOID = propertyTypePOID;
        _entityTypePOID = entityTypePOID;
        _defaultValue = defaultValue;
        m_Null = false;
    }

    public long PropertyPOID
    {
        get{return _propertyPOID;}
        set{_propertyPOID=value;}
    }

    public string Name
    {
        get{return _name;}
        set{_name=value;}
    }

    public long PropertyTypePOID
    {
        get{return _propertyTypePOID;}
        set{_propertyTypePOID=value;}
    }

    public long EntityTypePOID
    {
        get{return _entityTypePOID;}
        set{_entityTypePOID=value;}
    }

    public string DefaultValue
    {
        get{return _defaultValue;}
        set{_defaultValue=value;}
    }

    public override string ToString()
    {
        return "PropertyPOID: "+_propertyPOID +
               "Name: "+ _name +
               "PropertyTypePOID: "+_propertyTypePOID+
               "EntityTypePOID: "+_entityTypePOID+
               "DefaultValue: "+_defaultValue;
    }

    public bool IsNull
    {
        get
        {
            return m_Null;
        }
    }

    public static Property Null
    {
        get
        {
            Property h = new Property();
            h.m_Null = true;
            return h;
        }
    }

    public static Property Parse(SqlString s)
    {
        if (s.IsNull)
            return Null;
        Property u = new Property();
        
        return u;
    }
}


