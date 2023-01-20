namespace Fabrica.Models.Serialization;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ExcludeEmptyAttribute: Attribute
{
}