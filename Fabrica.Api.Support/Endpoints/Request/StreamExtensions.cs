/*

MIT License

Copyright (c) 2017 Jonathan Channon

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


namespace Fabrica.Api.Support.Endpoints.Request;

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public static class StreamExtensions
{
    /// <summary>
    /// Gets the <see cref="HttpRequest" /> Body <see cref="Stream"/> as <see cref="string"/> asynchronously in the optional <see cref="Encoding"/>
    /// </summary>
    /// <param name="stream">Current <see cref="Stream"/></param>
    /// <param name="encoding">The character encoding to use or <see cref="Encoding.UTF8"/> by default</param>
    /// <param name="cancellationToken">The cancellation instruction if required</param>
    /// <returns>Awaited <see cref="Task{String}"/></returns>
    public static async Task<string> AsStringAsync(this Stream stream, Encoding encoding = null, CancellationToken cancellationToken = default)
    {
        using (var reader = new StreamReader(stream, encoding ?? Encoding.UTF8, leaveOpen: true))
        {
            var readStream = await reader.ReadToEndAsync();

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            return readStream;
        }
    }
}