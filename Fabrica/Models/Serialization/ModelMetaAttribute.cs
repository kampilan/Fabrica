namespace Fabrica.Models.Serialization;

public enum PropertyScope { Mutable, Immutable, Exclude }

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ModelMetaAttribute : Attribute
{

    public string Name { get; set; } = string.Empty;

    public PropertyScope Scope { get; set; } = PropertyScope.Mutable;

    public bool Ignore { get; set; } = false;

}