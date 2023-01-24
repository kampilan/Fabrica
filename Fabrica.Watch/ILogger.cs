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

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global


using System.Runtime.CompilerServices;
using Fabrica.Watch.Sink;


namespace Fabrica.Watch;

public interface ILogger: IDisposable
{

    ILogEvent CreateEvent(Level level, object? title);
    ILogEvent CreateEvent(Level level, object? title, PayloadType type, string payload);
    ILogEvent CreateEvent(Level level, object? title, object payload);
    ILogEvent CreateEvent(Level level, object? title, Exception ex, object context);

    void LogEvent(  ILogEvent logEvent );


    bool IsTraceEnabled { get; }
    bool IsDebugEnabled { get; }
    bool IsInfoEnabled { get; }
    bool IsWarningEnabled { get; }
    bool IsErrorEnabled { get; }


    void Trace( object? message );
    void Trace( Func<string> expression );
    void Trace( Exception ex, object? message = null);

    void TraceFormat( string template, params object[] args);
    void TraceFormat( Exception ex, string template, params object[] args);


    void Debug( object? message );
    void Debug( Func<string> expression);
    void Debug( Exception ex, object? message = null);

    void DebugFormat( string template, params object[] args );
    void DebugFormat( Exception ex, string template, params object[] args );


    void Info( object? message);
    void Info( Func<string> expression);
    void Info( Exception ex, object? message = null );

    void InfoFormat( string template, params object[] args);
    void InfoFormat( Exception ex, string template, params object[] args );


    void Warning( object? message );
    void Warning( Func<string> expression );
    void Warning( Exception ex, object? message = null);
    void WarningWithContext( Exception ex, object context, object? message = null);

    void WarningFormat( string template, params object[] args);
    void WarningFormat( Exception ex, string template, params object[] args );


    void Error( object? message );
    void Error( Func<string> expression);
    void Error(  Exception ex, object? message = null);
    void ErrorWithContext( Exception ex, object context, object? message = null);

    void ErrorFormat( string template, params object[] args);
    void ErrorFormat( Exception ex, string template, params object[] args);


    void EnterMethod( [CallerMemberName] string name = "" );
    void LeaveMethod( [CallerMemberName] string name = "" );

    void EnterScope( string name );
    void LeaveScope( string name );

    void Inspect(object name, object? value);

    void LogSql( string title, string? sql );
    void LogXml( string title, string? xml, bool pretty = true );
    void LogJson( string title, string? json, bool pretty = true );
    void LogYaml( string title, string? yaml );

    void LogObject( string title, object? source );

    IDisposable BeginScope<TState>( TState state );

}