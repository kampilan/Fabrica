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


namespace Fabrica.Api.Support.Endpoints.Response;

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Negotiation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

public static class ResponseExtensions
{
    /// <summary>
    /// Executes content negotiation on current <see cref="HttpResponse"/>, utilizing an accepted media type if possible and defaulting to "application/json" if none found.
    /// </summary>
    /// <param name="response">Current <see cref="HttpResponse"/></param>
    /// <param name="obj">View model</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"/></returns>
    public static Task Negotiate(this HttpResponse response, object obj, CancellationToken cancellationToken = default)
    {
        var negotiators = response.HttpContext.RequestServices.GetServices<IResponseNegotiator>().ToList();
        IResponseNegotiator negotiator = null;

        MediaTypeHeaderValue.TryParseList(response.HttpContext.Request.Headers["Accept"], out var accept);
        if (accept != null)
        {
            var ordered = accept.OrderByDescending(x => x.Quality ?? 1);

            foreach (var acceptHeader in ordered)
            {
                negotiator = negotiators.FirstOrDefault(x => x.CanHandle(acceptHeader));
                if (negotiator != null)
                {
                    break;
                }
            }
        }

        if (negotiator == null)
        {
            negotiator = negotiators.First(x => x.CanHandle(new MediaTypeHeaderValue("application/json")));
        }

        return negotiator.Handle(response.HttpContext.Request, response, obj, cancellationToken);
    }

    /// <summary>
    /// Returns a Json response
    /// </summary>
    /// <param name="response">Current <see cref="HttpResponse"/></param>
    /// <param name="obj">View model</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="Task"/></returns>
    public static Task AsJson(this HttpResponse response, object obj, CancellationToken cancellationToken = default)
    {
        var negotiators = response.HttpContext.RequestServices.GetServices<IResponseNegotiator>();

        var negotiator = negotiators.First(x => x.CanHandle(new MediaTypeHeaderValue("application/json")));

        return negotiator.Handle(response.HttpContext.Request, response, obj, cancellationToken);
    }

    /// <summary>
    /// Copy a stream into the response body
    /// </summary>
    /// <param name="response">Current <see cref="HttpResponse"/></param>
    /// <param name="source">The <see cref="Stream"/> to copy from</param>
    /// <param name="contentType">The content type for the response</param>
    /// <param name="contentDisposition">The content disposition to allow file downloads</param>
    /// <returns><see cref="Task"/></returns>
    public static Task FromStream(this HttpResponse response, Stream source, string contentType, ContentDisposition contentDisposition = null)
    {
        var contentLength = source.Length;

        response.Headers["Accept-Ranges"] = "bytes";

        response.ContentType = contentType;

        if (contentDisposition != null)
        {
            response.Headers["Content-Disposition"] = contentDisposition.ToString();
        }

        if (RangeHeaderValue.TryParse(response.HttpContext.Request.Headers["Range"].ToString(), out var rangeHeader))
        {
            //Server should return multipart/byteranges; if asking for more than one range but pfft...
            var rangeStart = rangeHeader.Ranges.First().From;
            var rangeEnd = rangeHeader.Ranges.First().To ?? contentLength - 1;

            if (!rangeStart.HasValue || rangeEnd > contentLength - 1)
            {
                response.StatusCode = (int)HttpStatusCode.RequestedRangeNotSatisfiable;
                return Task.CompletedTask;
            }

            response.Headers["Content-Range"] = $"bytes {rangeStart}-{rangeEnd}/{contentLength}";
            response.StatusCode = (int)HttpStatusCode.PartialContent;
            if (!source.CanSeek)
            {
                throw new InvalidOperationException("Sending Range Responses requires a seekable stream eg. FileStream or MemoryStream");
            }

            source.Seek(rangeStart.Value, SeekOrigin.Begin);
            return StreamCopyOperation.CopyToAsync(source, response.Body, rangeEnd - rangeStart.Value + 1, 65536, response.HttpContext.RequestAborted);
        }

        return StreamCopyOperation.CopyToAsync(source, response.Body, default, 65536, response.HttpContext.RequestAborted);
    }
}