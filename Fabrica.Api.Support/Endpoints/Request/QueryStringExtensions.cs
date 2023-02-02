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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

public static class QueryStringExtensions
{
    /// <summary>
    /// Retrieve strongly typed query parameter value for given key
    /// </summary>
    /// <typeparam name="T">Query param type</typeparam>
    /// <param name="query"><see cref="IQueryCollection"/></param>
    /// <param name="key">Query param key</param>
    /// <param name="defaultValue">Default value if key not found</param>
    /// <returns>Query parameter value</returns>
    public static T As<T>(this IQueryCollection query, string key, T defaultValue = default)
    {
        var value = query[key].FirstOrDefault();

        if (value == null)
        {
            return defaultValue;
        }

        return value.ConvertTo<T>();
    }

    /// <summary>
    /// Retrieve strongly typed query parameter values for given key
    /// </summary>
    /// <typeparam name="T">Query param type</typeparam>
    /// <param name="query"><see cref="IQueryCollection"/></param>
    /// <param name="key">Query param key</param>
    /// <returns><see cref="IEnumerable{T}"/></returns>
    public static IEnumerable<T> AsMultiple<T>(this IQueryCollection query, string key)
    {
        var values = query[key];

        var splitValues = values.SelectMany(x => x.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

        return splitValues.ConvertMultipleTo<T>();
    }
}