using Fabrica.Watch.Sink;
using Newtonsoft.Json;

namespace Fabrica.Watch;

public class NewtonsoftWatchObjectSerializer: IWatchObjectSerializer
{

    static NewtonsoftWatchObjectSerializer()
    {

        var js = new JsonSerializerSettings
        {

            DateParseHandling = DateParseHandling.DateTime,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Formatting = Formatting.Indented,
            ContractResolver = new WatchContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,

            Error = delegate (object? _, Newtonsoft.Json.Serialization.ErrorEventArgs args)
            {
                args.ErrorContext.Handled = true;
            }

        };

        Settings = js;

        NullSource = JsonConvert.SerializeObject(new {IsNull = true}, Settings);

    }

    private static string NullSource { get; }
    private static JsonSerializerSettings Settings { get; }


    public (PayloadType type, string payload) Serialize(object? source)
    {

        if (source is null)
            return (PayloadType.None, "");

        var json = JsonConvert.SerializeObject(source, Settings);

        return (PayloadType.Json, json);

    }

}