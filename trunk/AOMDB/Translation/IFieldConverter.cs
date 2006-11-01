namespace Translation
{
    /// <summary>
    /// Interface specifying methods to convert between
    /// field values and values suitable for Property
    /// instances. 
    /// </summary>
    interface IFieldConverter {
        object ToPropertyValue(string propertyValue);
        string ToValueString(object fieldValue);
    }
}