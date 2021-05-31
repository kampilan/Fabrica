using System;

namespace Fabrica.Models.Serialization
{

    public enum RtoMutability { Always, CreateOnly, Never }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RtoAttribute : Attribute
    {

        public RtoMutability Mutability { get; set; } = RtoMutability.Always;

    }


}
