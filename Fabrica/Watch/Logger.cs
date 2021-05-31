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

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using Fabrica.Exceptions;
using Fabrica.Rules.Exceptions;
using Fabrica.Utilities.Drawing;
using Fabrica.Watch.Sink;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace Fabrica.Watch
{


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
                var le = CreateEvent(Level.Debug, CurrentScope);
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

        internal IEventSink Sink { get; set; }

        internal IList<string> Retro { get; set; }

        internal string Tenant { get; set; }
        internal string Subject { get; set; }
        internal string Tag { get; set; }

        internal string Category { get; set; }
        internal string CorrelationId { get; set; }

        internal Level Level { get; set; }
        internal Color Color { get; set; }


        [NotNull]
        public virtual  ILogEvent CreateEvent( Level level, object title )
        {

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
                Occurred      = DateTime.UtcNow
            };

            return le;

        }

        [NotNull]
        public virtual ILogEvent CreateEvent( Level level, object title, PayloadType type, string payload )
        {

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
                Type          = type,
                Payload       = payload
            };

            return le;

        }


        [NotNull]
        public virtual ILogEvent CreateEvent(Level level, object title, object payload )
        {

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
                Type          = PayloadType.Json
            };

            le.ToPayload( payload );

            return le;

        }


        [NotNull]
        public virtual ILogEvent CreateEvent(Level level, object title, Exception ex, [CanBeNull] object context )
        {

            var builder = new StringBuilder();
            builder.AppendLine($"Message: {title}");
            builder.AppendLine("");


            if( Retro != null && Retro.Count > 0 )
            {
                builder.AppendLine("--- Retro --------------------------------------------");
                foreach (var msg in Retro)
                    builder.AppendLine( msg );
                builder.AppendLine();
            }


            if( context != null )
            {
                var json = Watch.Sink.LogEvent.ToJson(context);
                builder.AppendLine("--- Context -----------------------------------------");
                builder.AppendLine(json);
                builder.AppendLine();
            }


            builder.AppendLine("--- Exception ---------------------------------------");
            var inner = ex;
            while( inner != null )
            {

                builder.AppendLine($" Exception: {inner.GetType().FullName} - {inner.Message}");

                if( inner is ExternalException ee )
                {
                    builder.AppendLine($"    ErrorKind:   {ee.Kind}");
                    builder.AppendLine($"    ErrorCode:   {ee.ErrorCode}");
                    builder.AppendLine($"    Explanation: {ee.Explanation}");
                }

                if( inner is ViolationsExistException ve )
                {
                    builder.AppendLine("--- Violations   --------------------------------------");
                    foreach (var ev in ve.Violations)
                        builder.AppendLine($"    {ev.Group} - {ev.Explanation}");
                }

                builder.AppendLine();
                builder.AppendLine("--- Stack Trace --------------------------------------");
                builder.AppendLine(inner.StackTrace);
                builder.AppendLine("------------------------------------------------------");

                inner = inner.InnerException;

            }


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
                Type          = PayloadType.Text,
                Payload       = builder.ToString()
            };

            return le;

        }



        public virtual bool IsTraceEnabled => Level == Level.Trace;

        public virtual bool IsDebugEnabled => Level <= Level.Debug;

        public virtual bool IsInfoEnabled => Level <= Level.Info;

        public virtual bool IsWarningEnabled => Level <= Level.Warning;

        public virtual bool IsErrorEnabled => Level <= Level.Error;

        public virtual void LogEvent( ILogEvent logEvent )
        {
            Sink.Accept( logEvent );
        }

        public virtual void Trace( object message )
        {

            if (!IsTraceEnabled)
                return;

            var le = CreateEvent(Level.Trace, message );
            LogEvent( le );

        }

        public virtual void Trace( Func<string> expression )
        {

            if (!IsTraceEnabled)
                return;

            Trace( expression() );

        }

        public virtual void Trace(Exception ex, object message = null)
        {

            if (!IsTraceEnabled)
                return;

            var le = CreateEvent( Level.Trace, message, ex, null );

            LogEvent(le);

        }

        public virtual void TraceFormat(string template, params object[] args)
        {

            if (!IsTraceEnabled)
                return;

            var title = string.Format(template, args);

            Trace( title );

        }

        public virtual void TraceFormat( Exception ex, string template, params object[] args )
        {

            if (!IsTraceEnabled)
                return;

            var title = string.Format(template, args);

            Trace( ex, title );

        }



        public virtual void Debug( object message )
        {

            if (!IsDebugEnabled)
            {
                if( Retro != null && message != null )
                    Retro.Add( message.ToString() );
                return;
            }


            var le = CreateEvent(Level.Debug, message);
            LogEvent(le);

        }

        public virtual void Debug( Func<string> expression )
        {

            if (!IsDebugEnabled)
            {
                Retro?.Add(expression());
                return;
            }


            Debug(expression());

        }

        public virtual void Debug( Exception ex, [CanBeNull] object message = null )
        {

            if (!IsDebugEnabled)
                return;

            var le = CreateEvent( Level.Debug, message, ex, null );

            LogEvent(le);

        }

        public virtual void DebugFormat(string template, params object[] args)
        {

            if (!IsDebugEnabled)
            {
                Retro?.Add(string.Format(template, args));
                return;
            }

            var title = string.Format(template, args);

            Debug(title);

        }

        public virtual void DebugFormat( Exception ex, string template, params object[] args )
        {

            if (!IsDebugEnabled)
                return;

            var title = string.Format(template, args);

            Debug(ex, title);

        }



        public virtual void Info(object message)
        {

            if (!IsInfoEnabled)
                return;

            var le = CreateEvent(Level.Info, message);
            LogEvent(le);

        }

        public virtual void Info(Func<string> expression)
        {

            if (!IsInfoEnabled)
                return;

            Info(expression());

        }

        public virtual void Info(Exception ex, object message = null)
        {

            if (!IsInfoEnabled)
                return;

            var le = CreateEvent(Level.Info, message, ex, null);

            LogEvent(le);

        }

        public virtual void InfoFormat(string template, params object[] args)
        {

            if (!IsInfoEnabled)
                return;

            var title = string.Format(template, args);

            Info(title);

        }

        public virtual void InfoFormat(Exception ex, string template, params object[] args)
        {

            if (!IsInfoEnabled)
                return;

            var title = string.Format(template, args);

            Info(ex, title);

        }



        public virtual void Warning(object message)
        {

            if (!IsWarningEnabled)
                return;

            var le = CreateEvent(Level.Warning, message);
            LogEvent(le);

        }

        public virtual void Warning(Func<string> expression)
        {

            if (!IsWarningEnabled)
                return;

            var message = expression();

            Warning( message );

        }

        public virtual void Warning( Exception ex, object message = null )
        {

            if (!IsWarningEnabled)
                return;

            var le = CreateEvent(Level.Warning, message, ex, null);

            LogEvent(le);

        }

        public virtual void WarningWithContext( Exception ex, object context, object message = null )
        {

            if (!IsWarningEnabled)
                return;

            var le = CreateEvent( Level.Warning, message, ex, context );

            LogEvent(le);


        }

        public virtual void WarningFormat(string template, params object[] args)
        {

            if (!IsWarningEnabled)
                return;

            var title = string.Format(template, args);

            Warning(title);

        }

        public virtual void WarningFormat(Exception ex, string template, params object[] args)
        {

            if (!IsWarningEnabled)
                return;

            var title = string.Format(template, args);

            Info(ex, title);

        }



        public virtual void Error( object message )
        {

            if (!IsErrorEnabled)
                return;

            var le = CreateEvent(Level.Error, message);
            LogEvent(le);

        }

        public virtual void Error( Func<string> expression )
        {

            if (!IsErrorEnabled)
                return;

            var message = expression();

            Error(message);

        }

        public virtual void Error(Exception ex, object message = null)
        {

            if (!IsErrorEnabled)
                return;

            var le = CreateEvent(Level.Error, message, ex, null);

            LogEvent(le);

        }

        public virtual void ErrorWithContext(Exception ex, object context, object message = null)
        {

            if (!IsErrorEnabled)
                return;

            var le = CreateEvent( Level.Error, message, ex, context );

            LogEvent(le);

        }

        public virtual void ErrorFormat(string template, params object[] args)
        {

            if (!IsErrorEnabled)
                return;

            var title = string.Format(template, args);

            Error(title);

        }

        public virtual void ErrorFormat( Exception ex, string template, params object[] args )
        {

            if (!IsErrorEnabled)
                return;

            var title = string.Format(template, args);

            Error(ex, title);

        }


        private string CurrentScope { get; set; } = "";


        public virtual void EnterMethod( [CallerMemberName] string name = "" )
        {

            if (!IsDebugEnabled)
                return;

            EnterScope( $"{Category}.{name}" );

        }

        public virtual void LeaveMethod( [CallerMemberName] string name = "" )
        {

            if (!IsDebugEnabled)
                return;

            LeaveScope( $"{Category}.{name}" );

        }

        public virtual void EnterScope( string name )
        {

            if (!IsDebugEnabled)
                return;

            CurrentScope = name;

            var le = CreateEvent(Level.Debug, name );
            le.Nesting = 1;

            LogEvent(le);

        }

        public virtual void LeaveScope( string name )
        {

            if (!IsDebugEnabled)
                return;

            CurrentScope = "";

            var le = CreateEvent(Level.Debug, name);
            le.Nesting = -1;

            LogEvent(le);

            Dispose();

        }

        public virtual void Inspect(object name, object value)
        {

            if (!IsDebugEnabled)
            {
                Retro?.Add( $"Variable: {name} = ({value})" );
                return;
            }

            var le = CreateEvent(Level.Debug, $"Variable: {name} = ({value})");

            LogEvent( le );

        }


        public virtual void LogSql(string title, string sql)
        {

            if (!IsDebugEnabled)
                return;

            var le = CreateEvent(Level.Debug, title, PayloadType.Sql, sql??"" );

            LogEvent( le );

        }

        public virtual void LogXml( string title, string xml, bool pretty = true )
        {

            if (!IsDebugEnabled)
                return;

            if( pretty && (xml != null) && !string.IsNullOrWhiteSpace(xml) )
                xml = MakeXmlPretty(xml);

            var le = CreateEvent(Level.Debug, title, PayloadType.Xml, xml??"" );

            LogEvent(le);

        }

        public virtual void LogJson(string title, string json, bool pretty = true)
        {

            if (!IsDebugEnabled)
                return;

            if (pretty && json != null && !string.IsNullOrWhiteSpace(json) )
                json = MakeJsonPretty(json);

            var le = CreateEvent(Level.Debug, title, PayloadType.Json, json ?? "");

            LogEvent(le);

        }

        public void LogYaml(string title, string yaml)
        {

            if (!IsDebugEnabled)
                return;

            var le = CreateEvent(Level.Debug, title, PayloadType.Yaml, yaml ?? "");

            LogEvent(le);

        }

        public virtual void LogObject(string title, object source)
        {

            if( !IsDebugEnabled )
                return;

            var le = CreateEvent( Level.Debug, title, source??new {} );
            LogEvent(le);

        }

        public virtual IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }


        protected virtual string MakeXmlPretty( [NotNull] string xml )
        {

            var pretty = xml;
            try
            {

                var document = new XmlDocument();
                document.LoadXml(xml);

                using (var writer = new StringWriter())
                using (var xw = new XmlTextWriter(writer))
                {
                    xw.Formatting = System.Xml.Formatting.Indented;
                    document.Save(xw);
                    writer.Flush();
                    pretty = writer.ToString();
                }

            }
            catch
            {
                // ignored
            }

            return pretty;

        }

        protected virtual string MakeJsonPretty( [NotNull] string json )
        {

            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(json));

            var pretty = json;
            try
            {
                var tok = JToken.Parse(json);
                pretty = tok.ToString(Formatting.Indented);
            }
            catch
            {
                // ignored
            }

            return pretty;

        }




    }

}
