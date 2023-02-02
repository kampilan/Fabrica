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

namespace Fabrica.Api.Support.Endpoints;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public static class RouteExtensions
{

    private static async ValueTask<object> RouteHandler<T>(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var param = (T)context.Arguments.FirstOrDefault(x => x?.GetType() == typeof(T));
        /*
                var result = context.HttpContext.Request.Validate(param);
                if (!result.IsValid)
                {
                    var problemDetailsErrors = new Dictionary<string, object> { { "errors", result.GetFormattedErrors() } };
                    return Results.Problem(statusCode: 422, extensions: problemDetailsErrors);
                }
        */
        return await next(context);
    }

    /// <summary>
    /// Add a POST handler that will validate your model and return 422 if validation fails
    /// </summary>
    /// <param name="endpoints"></param>
    /// <param name="pattern">The route path pattern</param>
    /// <param name="handler">The route handler</param>
    /// <typeparam name="T">The model to validate</typeparam>
    /// <returns></returns>
    public static RouteHandlerBuilder MapPost<T>(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern, Delegate handler)
    {
        return endpoints.MapPost(pattern, handler).AddEndpointFilter(async (context, next) => await RouteHandler<T>(context, next));
    }

    /// <summary>
    /// Add a PUT handler that will validate your model and return 422 if validation fails
    /// </summary>
    /// <param name="endpoints"></param>
    /// <param name="pattern">The route path pattern</param>
    /// <param name="handler">The route handler</param>
    /// <typeparam name="T">The model to validate</typeparam>
    /// <returns></returns>
    public static RouteHandlerBuilder MapPut<T>(this IEndpointRouteBuilder endpoints, [StringSyntax("Route")] string pattern, Delegate handler)
    {
        return endpoints.MapPut(pattern, handler).AddEndpointFilter(async (context, next) => await RouteHandler<T>(context, next));
    }

}
