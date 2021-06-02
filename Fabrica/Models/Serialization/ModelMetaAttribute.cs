using System;

namespace Fabrica.Models.Serialization
{

    public enum PropertyMutability { Always, CreateOnly, Never }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ModelMetaAttribute : Attribute
    {

        public PropertyMutability Mutability { get; set; } = PropertyMutability.Always;

        public bool Ignore { get; set; } = false;

    }


}
