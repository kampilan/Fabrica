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

using System.Text;
using Fabrica.Watch.Sink;
using Gurock.SmartInspect;

namespace Fabrica.Watch.Realtime;

public class RealtimeSink: IEventSink
{


    private SmartInspect Si { get; set; } = null!;

    public void Start()
    {

        Si = new SmartInspect("Fabrica")
        {
            Connections = "tcp(host=localhost, reconnect=true, reconnect.interval=10)",
            Enabled = true,
            Level = Gurock.SmartInspect.Level.Debug
        };


    }

    public void Stop()
    {
        Si.Enabled = false;
        Si.Dispose();
    }

    public Task Accept(ILogEvent logEvent)
    {

        var entry = _mapToLogEntry(logEvent);
        Si.SendLogEntry( entry );

        return Task.CompletedTask;

    }

    public Task Accept(IEnumerable<ILogEvent> batch)
    {

        foreach( var le in batch )
            Accept(le);

        return Task.CompletedTask;

    }


    private static Gurock.SmartInspect.Level _mapToSILevel(Level level)
    {
        return level switch
        {
            Level.Trace => Gurock.SmartInspect.Level.Debug,
            Level.Debug => Gurock.SmartInspect.Level.Debug,
            Level.Info => Gurock.SmartInspect.Level.Message,
            Level.Warning => Gurock.SmartInspect.Level.Warning,
            Level.Error => Gurock.SmartInspect.Level.Error,
            _ => Gurock.SmartInspect.Level.Fatal
        };
    }


    private static LogEntry _mapToLogEntry( ILogEvent le )
    {


        var entryType = LogEntryType.Debug;
        switch (le.Level)
        {
            case Level.Trace:
                entryType = LogEntryType.Debug;
                break;

            case Level.Debug:
                entryType = LogEntryType.Debug;
                break;

            case Level.Info:
                entryType = LogEntryType.Message;
                break;

            case Level.Warning:
                entryType = LogEntryType.Warning;
                break;

            case Level.Quiet:
            case Level.Error:
                entryType = LogEntryType.Error;
                break;

        }


        if (le.Nesting == 1)
            entryType = LogEntryType.EnterMethod;
        else if (le.Nesting == -1)
            entryType = LogEntryType.LeaveMethod;


        MemoryStream data = null!;
        var viewerId  = ViewerId.Title;
        if( !string.IsNullOrWhiteSpace(le.Payload) )
        {

            var buf = Convert.FromBase64String(le.Payload);
            var clear = Encoding.ASCII.GetString(buf);

            data = new MemoryStream();
            var writer = new StreamWriter(data);
            writer.Write(clear);
            writer.Flush();
            data.Seek(0, SeekOrigin.Begin);

            switch (le.Type)
            {
                case PayloadType.Json:
                    entryType = LogEntryType.Source;
                    viewerId  = ViewerId.JavaScriptSource;
                    break;
                case PayloadType.Sql:
                    entryType = LogEntryType.Source;
                    viewerId  = ViewerId.SqlSource;
                    break;
                case PayloadType.Xml:
                    entryType = LogEntryType.Source;
                    viewerId  = ViewerId.XmlSource;
                    break;
                case PayloadType.Text:
                    entryType = LogEntryType.Source;
                    viewerId  = ViewerId.Data;
                    break;
                case PayloadType.Yaml:
                    entryType = LogEntryType.Source;
                    viewerId = ViewerId.JavaScriptSource;
                    break;
            }

        }

        var entry = new LogEntry
        {
            AppName       = "Fabrica",
            Level         = _mapToSILevel(le.Level),
            SessionName   = le.Category,
            Title         = le.Title,
            LogEntryType  = entryType,
            ViewerId      = viewerId,
            Timestamp     = le.Occurred,
            Data          = data
        };


        var rgb = le.Color & 0xffffff;
        entry.Color = System.Drawing.Color.FromArgb(rgb);

        return entry;

    }



}