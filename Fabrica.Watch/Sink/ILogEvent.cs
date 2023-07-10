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

namespace Fabrica.Watch.Sink;

public interface ILogEvent: IDisposable
{

    string Category { get; set; }
    string CorrelationId { get; set; }

    string Title { get; set; }

    string Tenant { get; set; }
    string Subject { get; set; }
    string Tag { get; set; }

    Level Level { get; set; }
    int Color { get; set; }
    int Nesting { get; set; }

    DateTime Occurred { get; set; }

    PayloadType Type { get; set; }

    object? Object { get; set; }

    Exception? Error { get; set; }
    object? ErrorContext { get; set; }

    string? Payload { get; set; }
    string? Base64 { get; set; }


}