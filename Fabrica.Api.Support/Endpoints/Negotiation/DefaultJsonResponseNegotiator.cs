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

namespace Fabrica.Api.Support.Endpoints.Negotiation;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Negotiation;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

public class DefaultJsonResponseNegotiator : IResponseNegotiator
{
    private readonly JsonSerializerOptions jsonSettings;

    public DefaultJsonResponseNegotiator()
    {
        jsonSettings = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public bool CanHandle(MediaTypeHeaderValue accept)
    {
        return accept.MediaType.ToString().IndexOf("json", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public async Task Handle(HttpRequest req, HttpResponse res, object model, CancellationToken cancellationToken)
    {
        res.ContentType = "application/json; charset=utf-8";

        await JsonSerializer.SerializeAsync(res.Body, model, model == null ? typeof(object) : model.GetType(), jsonSettings, cancellationToken);
    }
}