using Newtonsoft.Json;

namespace Fabrica.Models.Serialization
{

    public static class ModelJsonSerializerSettings
    {


        public static JsonSerializerSettings ForRto { get; } = new JsonSerializerSettings
        {
            ContractResolver           = new ModelContractResolver(),
            DefaultValueHandling       = DefaultValueHandling.Populate,
            NullValueHandling          = NullValueHandling.Ignore,
            DateTimeZoneHandling       = DateTimeZoneHandling.Utc,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling      = ReferenceLoopHandling.Serialize
        };

        public static JsonSerializerSettings ForRtoNoDefaults { get; } = new JsonSerializerSettings
        {
            ContractResolver           = new ModelContractResolver(),
            DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate,
            NullValueHandling          = NullValueHandling.Ignore,
            DateTimeZoneHandling       = DateTimeZoneHandling.Utc,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling      = ReferenceLoopHandling.Serialize
        };


        public static JsonSerializerSettings ForRtoExplorer { get; } = new JsonSerializerSettings
        {
            ContractResolver           = new ModelExplorerContractResolver(),
            DefaultValueHandling       = DefaultValueHandling.Populate,
            NullValueHandling          = NullValueHandling.Ignore,
            DateTimeZoneHandling       = DateTimeZoneHandling.Utc
        };

        public static JsonSerializerSettings ForRtoExplorerNoDefaults { get; } = new JsonSerializerSettings
        {
            ContractResolver           = new ModelExplorerContractResolver(),
            DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate,
            NullValueHandling          = NullValueHandling.Ignore,
            DateTimeZoneHandling       = DateTimeZoneHandling.Utc
        };


    }


}
