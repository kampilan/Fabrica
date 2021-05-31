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

using System;
using System.Runtime.CompilerServices;
using Fabrica.Watch.Sink;
using JetBrains.Annotations;

namespace Fabrica.Watch
{

    public interface ILogger: IDisposable
    {

        ILogEvent CreateEvent(Level level, [CanBeNull] object title);
        ILogEvent CreateEvent(Level level, [CanBeNull] object title, PayloadType type, [NotNull] string payload);
        ILogEvent CreateEvent(Level level, [CanBeNull] object title, [NotNull] object payload);
        ILogEvent CreateEvent(Level level, [CanBeNull] object title, Exception ex, object context);

        void LogEvent( [NotNull] ILogEvent logEvent);


        bool IsTraceEnabled { get; }
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarningEnabled { get; }
        bool IsErrorEnabled { get; }


        void Trace( [CanBeNull] object message );
        void Trace( [NotNull] Func<string> expression );
        void Trace( [NotNull] Exception ex, [CanBeNull]object message = null);

        [StringFormatMethod("template")]
        void TraceFormat( [NotNull] string template, params object[] args);
        [StringFormatMethod("template")]
        void TraceFormat( [NotNull] Exception ex, [NotNull] string template, params object[] args);


        void Debug( [CanBeNull] object message );
        void Debug( [NotNull] Func<string> expression);
        void Debug([NotNull] Exception ex, object message = null);

        [StringFormatMethod("template")]
        void DebugFormat( [NotNull] string template, params object[] args );
        [StringFormatMethod("template")]
        void DebugFormat( [NotNull] Exception ex, [NotNull] string template, params object[] args );


        void Info([CanBeNull] object message);
        void Info( [NotNull] Func<string> expression);
        void Info( [NotNull]Exception ex, [CanBeNull] object message = null );

        [StringFormatMethod("template")]
        void InfoFormat([NotNull] string template, params object[] args);
        [StringFormatMethod("template")]
        void InfoFormat( [NotNull] Exception ex, [NotNull] string template, params object[] args );


        void Warning( [CanBeNull] object message );
        void Warning( [NotNull] Func<string> expression);
        void Warning( [NotNull] Exception ex, [CanBeNull] object message = null);
        void WarningWithContext([NotNull] Exception ex, [NotNull] object context, [CanBeNull] object message = null);

        [StringFormatMethod("template")]
        void WarningFormat([NotNull] string template, params object[] args);
        [StringFormatMethod("template")]
        void WarningFormat( [NotNull] Exception ex, [NotNull] string template, params object[] args);


        void Error( [CanBeNull] object message );
        void Error( [NotNull] Func<string> expression);
        void Error( [NotNull] Exception ex, [CanBeNull] object message = null);
        void ErrorWithContext([NotNull] Exception ex, [NotNull] object context, [CanBeNull] object message = null);

        [StringFormatMethod("template")]
        void ErrorFormat([NotNull] string template, params object[] args);
        [StringFormatMethod("template")]
        void ErrorFormat( [NotNull] Exception ex, [NotNull] string template, params object[] args);


        void EnterMethod( [CallerMemberName] string name = "" );
        void LeaveMethod( [CallerMemberName] string name = "" );

        void EnterScope( [NotNull] string name );
        void LeaveScope( [NotNull] string name );

        void Inspect([NotNull] object name, [CanBeNull] object value);

        void LogSql( [NotNull] string title, [CanBeNull] string sql );
        void LogXml( [NotNull] string title, [CanBeNull] string xml, bool pretty = true );
        void LogJson( [NotNull] string title, [CanBeNull] string json, bool pretty = true );
        void LogYaml( [NotNull] string title, [CanBeNull] string yaml );

        void LogObject( [NotNull] string title, [CanBeNull] object source );

        IDisposable BeginScope<TState>( TState state );

    }

}
