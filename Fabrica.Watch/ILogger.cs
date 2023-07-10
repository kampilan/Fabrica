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


using Fabrica.Watch.Sink;


namespace Fabrica.Watch;

public interface ILogger: IDisposable
{

    string Category { get; }


    bool IsTraceEnabled { get; }
    bool IsDebugEnabled { get; }
    bool IsInfoEnabled { get; }
    bool IsWarningEnabled { get; }
    bool IsErrorEnabled { get; }


    ILogEvent CreateEvent(Level level, object? title);
    ILogEvent CreateEvent(Level level, object? title, PayloadType type, string? payload);
    ILogEvent CreateEvent(Level level, object? title, object? payload);
    ILogEvent CreateEvent( Level level, object? title, Exception ex, object? context );


    void LogEvent( ILogEvent logEvent );

    string GetCurrentScope();
    void SetCurrentScope(string name);

    IDisposable BeginScope<TState>( TState state );

}