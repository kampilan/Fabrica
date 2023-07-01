/*
The MIT License (MIT)

Copyright (c) 2020 The Kampilan Group Inc.

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

using System.Drawing;
using Fabrica.Watch.Sink;
using Fabrica.Watch.Utilities;

namespace Fabrica.Watch;

public class Logger : ILogger
{

    internal Logger(Action<Logger> onDispose)
    {
        OnDispose = onDispose;
    }

    private Action<Logger> OnDispose { get; }


    public void Dispose()
    {

        if( !string.IsNullOrWhiteSpace(CurrentScope) )
        {
            var le = CreateEvent( Level.Debug, CurrentScope );
            le.Nesting = -1;

            CurrentScope = "";

            LogEvent(le);
        }

        OnDispose(this);

    }

    internal void Config( IEventSink sink, bool retroOn, string tenant, string subject, string tag, string category, string correlationId, Level level, Color color )
    {

        Sink = sink;

        Retro = retroOn ? new List<string>() : null;

        Tenant  = tenant;
        Subject = subject;
        Tag     = tag;

        Category      = category;
        CorrelationId = correlationId;

        Level = level;
        Color = color;

    }


    public virtual bool IsTraceEnabled => Level == Level.Trace;

    public virtual bool IsDebugEnabled => Level <= Level.Debug;

    public virtual bool IsInfoEnabled => Level <= Level.Info;

    public virtual bool IsWarningEnabled => Level <= Level.Warning;

    public virtual bool IsErrorEnabled => Level <= Level.Error;



    internal IEventSink Sink { get; set; } = null!;


    public string Category { get; protected set; } = "";

    internal string Tenant { get; set; } = "";
    internal string Subject { get; set; } = "";
    internal string Tag { get; set; } = "";

    internal string CorrelationId { get; set; } = "";

    internal Level Level { get; set; }
    internal Color Color { get; set; }

    internal IList<string>? Retro { get; set; }

    public void AddToRetro(string message)
    {
        Retro?.Add(message);
    }


    public virtual ILogEvent CreateEvent( Level level, object? title )
    {

        if (string.IsNullOrWhiteSpace(CorrelationId))
            CorrelationId = Ulid.NewUlid();

        var le = new LogEvent
        {
            Tenant        = Tenant,
            Subject       = Subject,
            Tag           = Tag,
            Category      = Category,
            CorrelationId = CorrelationId,
            Level         = level,
            Color         = Color.ToArgb(),
            Title         = title?.ToString() ?? "",
            Occurred      = DateTime.UtcNow,
            Retro         = Retro
        };

        return le;

    }

    public virtual ILogEvent CreateEvent( Level level, object? title, PayloadType type, string? content )
    {

        if (string.IsNullOrWhiteSpace(CorrelationId))
            CorrelationId = Ulid.NewUlid();

        var le = new LogEvent
        {
            Tenant        = Tenant,
            Subject       = Subject,
            Tag           = Tag,
            Category      = Category,
            CorrelationId = CorrelationId,
            Level         = level,
            Color         = Color.ToArgb(),
            Title         = title?.ToString() ?? "",
            Occurred      = DateTime.UtcNow,
            Type          = PayloadType.None,
            Retro         = Retro
        };

        if( string.IsNullOrWhiteSpace(content) )
            return le;
       
        le.Type   = type;
        le.Payload = content;

        return le;

    }


    public virtual ILogEvent CreateEvent(Level level, object? title, object? obj )
    {

        if( string.IsNullOrWhiteSpace(CorrelationId) )
            CorrelationId = Ulid.NewUlid();

        var le = new LogEvent
        {
            Tenant        = Tenant,
            Subject       = Subject,
            Tag           = Tag,
            Category      = Category,
            CorrelationId = CorrelationId,
            Level         = level,
            Color         = Color.ToArgb(),
            Title         = title?.ToString() ?? "",
            Occurred      = DateTime.UtcNow,
            Type          = PayloadType.None,
            Retro         = Retro
        };

        if( obj is null )
            return le;

        le.Object = obj;

        return le;

    }


    public virtual ILogEvent CreateEvent(Level level, object? title, Exception ex,  object? context )
    {

        if( string.IsNullOrWhiteSpace(CorrelationId) )
            CorrelationId = Ulid.NewUlid();

        var le = new LogEvent
        {
            Tenant        = Tenant,
            Subject       = Subject,
            Tag           = Tag,
            Category      = Category,
            CorrelationId = CorrelationId,
            Level         = level,
            Color         = Color.ToArgb(),
            Title         = title?.ToString() ?? "",
            Occurred      = DateTime.UtcNow,
            Error         = ex,
            ErrorContext  = context,
            Retro         = Retro
        };

        return le;

    }


    public virtual void LogEvent( ILogEvent logEvent )
    {

        WatchFactoryLocator.Factory.Enrich(logEvent);

        Sink.Accept( logEvent );

    }


    private string CurrentScope { get; set; } = "";
    public string GetCurrentScope() => CurrentScope;
    public void SetCurrentScope(string name)
    {
        CurrentScope = name;
    }

    public virtual IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }



}