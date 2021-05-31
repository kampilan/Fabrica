using Newtonsoft.Json;

namespace Fabrica.Models.Serialization
{

    public static class RtoJsonSerializerSettings
    {


        public static JsonSerializerSettings ForRto { get; } = new JsonSerializerSettings
        {
            ContractResolver           = new RtoContractResolver(),
            DefaultValueHandling       = DefaultValueHandling.Populate,
            NullValueHandling          = NullValueHandling.Ignore,
            DateTimeZoneHandling       = DateTimeZoneHandling.Utc,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling      = ReferenceLoopHandling.Serialize
        };

        public static JsonSerializerSettings ForRtoNoDefaults { get; } = new JsonSerializerSettings
        {
            ContractResolver           = new RtoContractResolver(),
            DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate,
            NullValueHandling          = NullValueHandling.Ignore,
            DateTimeZoneHandling       = DateTimeZoneHandling.Utc,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling      = ReferenceLoopHandling.Serialize
        };


        public static JsonSerializerSettings ForRtoExplorer { get; } = new JsonSerializerSettings
        {
            ContractResolver           = new RtoExplorerContractResolver(),
            DefaultValueHandling       = DefaultValueHandling.Populate,
            NullValueHandling          = NullValueHandling.Ignore,
            DateTimeZoneHandling       = DateTimeZoneHandling.Utc
        };

        public static JsonSerializerSettings ForRtoExplorerNoDefaults { get; } = new JsonSerializerSettings
        {
            ContractResolver           = new RtoExplorerContractResolver(),
            DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate,
            NullValueHandling          = NullValueHandling.Ignore,
            DateTimeZoneHandling       = DateTimeZoneHandling.Utc
        };


    }


}
