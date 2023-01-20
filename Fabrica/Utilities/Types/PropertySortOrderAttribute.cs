namespace Fabrica.Utilities.Types;

[AttributeUsage(AttributeTargets.Property)]
public sealed class PropertySortOrder : Attribute
{

    public PropertySortOrder(int order)
    {
        Order = order;
    }
    public int Order { get; }



}