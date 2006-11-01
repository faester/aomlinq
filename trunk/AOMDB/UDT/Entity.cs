using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;

// example imports
using System.Xml;
using System.Data.Sql;
using System.Globalization;



[Serializable]
[Microsoft.SqlServer.Server.SqlUserDefinedType(Format.Native)]
public class Entity : INullable, IComparable, Microsoft.SqlServer.Server.IBinarySerialize
{
    private long _entityPOID;
    private long _entityTypePOID;
    private string _name;
    private byte[] _byteName;
    
    private bool m_Null;

    #region Constructors

    public Entity(long entityPOID, long entityTypePOID, String name)
    {
        _entityPOID = entityPOID;
        _entityTypePOID = entityTypePOID;
        _name = name;
        m_Null = false;
    }

    public Entity(byte[] bytes)
    {
        _byteName = bytes;
    }

    public Entity()
    {
    }

    #endregion Constructors

    #region Properties

    public long EntityPOID
    {
        get {return _entityPOID;}
        set {_entityPOID=value;}
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

    public byte[] ByteName
    {
        get{return _byteName;}

        set{_byteName=value;}
    }

    #endregion Properties

    
    public bool IsNull
    {
        get {return m_Null;}
    }

    public static Entity Null
    {
        get
        {
            Entity h = new Entity();
            h.m_Null = true;
            return h;
        }
    }

    public static Entity Parse(SqlString sqlString)
    {
        if (sqlString.IsNull) return Entity.Null;
        
        // TODO..

        // parse: 
        // entityPOID
        // entityTypePOID
        // name

        // create and return new instance:
        // return new Entity(entityPOID, entityTypePOID, name);

        Console.WriteLine("Parsing: {0}", sqlString.ToString()); // test

        return new Entity(); // tmp
    }

    // taken from eaxmple code
    public int CompareTo(object obj)
        {
            if (obj == null)
                return 1; //by definition

            Entity s = obj as Entity;

            if (s == null)
                throw new ArgumentException("the argument to compare is not a Utf8String");

            if (this.IsNull)
            {
                if (s.IsNull)
                    return 0;

                return -1;
            }

            if (s.IsNull)
                return 1;

            return this.ToString().CompareTo(s.ToString());
        }

    public SqlBinary Utf8Bytes
        {
            get
            {
                if (this.IsNull)
                    return SqlBinary.Null;

                if (this._byteName != null)
                    return this._byteName;

                if (this._name != null)
                {
                    this._byteName = System.Text.Encoding.UTF8.GetBytes(this._name);
                    return new SqlBinary(this._byteName);
                }

                throw new NotSupportedException("cannot return bytes for empty instance");
            }
            set
            {
                if (value.IsNull)
                {
                    this._byteName = null;
                    this._name = null;
                }
                else
                {
                    this._byteName = value.Value;
                    this._name = null;
                }
            }
        }

        /// <summary>
        /// Return a unicode string for this type.
        /// </summary>
        [Microsoft.SqlServer.Server.SqlMethod(IsDeterministic = true, IsPrecise = true, DataAccess = Microsoft.SqlServer.Server.DataAccessKind.None, SystemDataAccess = Microsoft.SqlServer.Server.SystemDataAccessKind.None)]
        public override string ToString()
        {
            if(this.IsNull) return null;

            if(this._byteName != null)
                this._name = System.Text.Encoding.UTF8.GetString(this._byteName);

            return "entityPOID: "+_entityPOID+", entityTypePOID: "+_entityTypePOID+", name: "+_name;
        }


        #region IBinarySerialize Members
        public void Write(System.IO.BinaryWriter w)
        {
            byte header = (byte)(this.IsNull ? 1 : 0);

            w.Write(header);
            if (header == 1)
                return;

            byte[] bytes = this.Utf8Bytes.Value;

            w.Write(bytes.Length);
            w.Write(bytes);
        }

        public void Read(System.IO.BinaryReader r)
        {
            byte header = r.ReadByte();

            if ((header & 1) > 0)
            {
                this._byteName = null;
                return;
            }

            int length = r.ReadInt32();

            this._byteName = r.ReadBytes(length);
        }
        #endregion
}


