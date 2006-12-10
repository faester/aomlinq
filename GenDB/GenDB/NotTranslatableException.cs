using System;
using System.Reflection;

public class NotTranslatableException : Exception
{
    FieldInfo fi;
    Type t;

    public NotTranslatableException(string msg, FieldInfo fi)
        : base(msg)
    {
        this.fi = fi;
    }

    public NotTranslatableException(string msg, Type t)
        : base(msg)
    {
        this.t = t;
    }

    public FieldInfo FieldInfo
    {
        get { return fi; }
    }

    public Type TypeInfo
    {
        get { return t; }
    }

    public override string ToString()
    {
        string res = Message;
        if (fi != null) { res += " (Conflicting Field: " + fi.ToString() + ")"; }
        if (t != null) { res += " (Conflicting Type: " + t.ToString() + ")"; }
        return res;
    }
}