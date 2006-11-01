using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

using System.Runtime.InteropServices;


//[Serializable]
//[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.Native)]
//[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
public struct EntityType : INullable
{
    private bool m_Null;
    private long _entityTypePOID;
    
    //[MarshalAs(UnmanagedType.BStr)] 
    private String _name;

    public EntityType(long entityTypePOID, string name)
    {
        _entityTypePOID = entityTypePOID;
        _name = name;
        m_Null = false;
    }

    public long EntityTypePOID
    {
        get{return _entityTypePOID;}
        set{_entityTypePOID=value;}
    }

    public string Name
    {
        get{return _name;}
        set{_name=value;}
    }

    public override string ToString()
    {
        return "EntityTypePOID: "+_entityTypePOID+", name: "+_name;
    }

    public bool IsNull
    {
        get{return m_Null;}
    }

    public static EntityType Null
    {
        get
        {
            EntityType h = new EntityType();
            h.m_Null = true;
            return h;
        }
    }

    public static EntityType Parse(SqlString s)
    {
        Console.WriteLine("UDT SqlString in EntityType: {0}",s.ToString());
        
        if (s.IsNull)
            return Null;
        EntityType u = new EntityType();

        return u;
    }
}


