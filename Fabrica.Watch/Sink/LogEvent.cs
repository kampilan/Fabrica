﻿/*
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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fabrica.Watch.Sink;

public class LogEvent: ILogEvent
{


    static LogEvent()
    {

        Settings = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.Preserve,
        };

    }

    private static JsonSerializerOptions Settings { get; }

    public static string ToJson( object source )
    {
        var json = JsonSerializer.Serialize(source, Settings);
        return json;
    }


    public string Category { get; set; } = "";
    public string CorrelationId { get; set; } = "";

    public string Title { get; set; } = "";

    public string Tenant { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Tag { get; set; } = "";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Level Level { get; set; } = Level.Trace;
    public int Color { get; set; } = 0;
    public int Nesting { get; set; } = 0;


    public DateTime Occurred { get; set; } = DateTime.UtcNow;


    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PayloadType Type { get; set; } = PayloadType.None;
    public string Payload { get; set; } = "";

    public void ToPayload( object source )
    {
        Type = PayloadType.Json;
        Payload = ToJson(source);
    }


}