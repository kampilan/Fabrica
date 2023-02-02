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
using System.ComponentModel;
using System.Globalization;
using System.Linq;

internal static class ConvertExtensions
{
    internal static T ConvertTo<T>(this object value, T defaultValue = default)
    {
        if (value != null)
        {
            try
            {
                var currentType = value.GetType();
                var newType = typeof(T);

                if (currentType.IsAssignableFrom(newType))
                {
                    return (T)value;
                }

                var stringValue = value as string;

                if (newType == typeof(DateTime))
                {
                    if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dateResult))
                    {
                        return (T)(object)dateResult;
                    }

                    return defaultValue;
                }

                if (stringValue != null)
                {
                    var converter = TypeDescriptor.GetConverter(newType);

                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        return (T)converter.ConvertFromInvariantString(stringValue);
                    }

                    return defaultValue;
                }

                var underlyingType = Nullable.GetUnderlyingType(newType) ?? newType;

                return (T)Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
            }
            catch
            {
                return defaultValue;
            }
        }

        return defaultValue;
    }

    internal static IEnumerable<T> ConvertMultipleTo<T>(this IEnumerable<string> values)
    {
        foreach (var value in values)
        {
            yield return value.ConvertTo<T>();
        }
    }

    internal static bool IsArray(this Type source)
    {
        return source.BaseType == typeof(Array);
    }

    internal static bool IsCollection(this Type source)
    {
        var collectionType = typeof(ICollection<>);

        return source.IsGenericType && source
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == collectionType);
    }

    internal static bool IsEnumerable(this Type source)
    {
        var enumerableType = typeof(IEnumerable<>);

        return source.IsGenericType && source.GetGenericTypeDefinition() == enumerableType;
    }
}