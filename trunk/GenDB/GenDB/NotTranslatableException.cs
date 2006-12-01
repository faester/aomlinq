using System;
using System.Reflection;

public class NotTranslatableException : Exception
{
    FieldInfo fi;
    public NotTranslatableException(string msg, FieldInfo fi)
        : base(msg)
    {
        this.fi = fi;
    }

    public FieldInfo FieldInfo
    {
        get { return fi; }
    }

    public override string ToString()
    {
        return Message + " (Conflicting Field: " + fi.ToString() + ")";
    }
}