/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Fabrica.Watch.Sink;

public class LogEvent: ILogEvent
{


    static LogEvent()
    {

        Settings = new JsonSerializerSettings
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

    }

    private static JsonSerializerSettings Settings { get; }

    public static string ToJson(object source)
    {
        var json = JsonConvert.SerializeObject(source, Settings);
        return json;
    }



    public string Category { get; set; } = "";
    public string CorrelationId { get; set; } = "";

    public string Title { get; set; } = "";

    public string Tenant { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Tag { get; set; } = "";

    [JsonConverter(typeof(StringEnumConverter))]
    public Level Level { get; set; } = Level.Trace;
    public int Color { get; set; } = 0;
    public int Nesting { get; set; } = 0;


    public DateTime Occurred { get; set; } = DateTime.UtcNow;


    [JsonConverter(typeof(StringEnumConverter))]
    public PayloadType Type { get; set; } = PayloadType.None;
    public string Payload { get; set; } = "";

    public void ToPayload( object source )
    {
        Type = PayloadType.Json;
        Payload = ToJson(source);
    }


}

public class WatchContractResolver : DefaultContractResolver
{


    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {

        var mem = base.CreateProperty(member, memberSerialization);

        if (!(member is PropertyInfo propInfo))
            return mem;

        if (propInfo.GetCustomAttribute<SensitiveAttribute>() != null)
            mem.ValueProvider = new SensitiveValueProvider(propInfo);

        return mem;

    }


}

public class SensitiveValueProvider : IValueProvider
{


    public SensitiveValueProvider(PropertyInfo propInfo)
    {
        PropInfo = propInfo;
    }


    private PropertyInfo PropInfo { get; }


    public void SetValue(object target, object value)
    {
        PropInfo.SetValue(target, value);
    }

    public object GetValue(object target)
    {

        var value = PropInfo.GetValue(target);
        var strVal = value?.ToString();

        var sub = $"Sensitive - HasValue: {!string.IsNullOrWhiteSpace(strVal)}";

        return sub;

    }


}