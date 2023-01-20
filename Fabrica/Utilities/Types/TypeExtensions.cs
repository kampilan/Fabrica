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

namespace Fabrica.Utilities.Types;

public static class TypeExtensions
{


    public static string ToHexString( this byte[] bytes )
    {

        if (bytes == null) throw new ArgumentNullException(nameof(bytes));

        var hex = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        return hex;

    }


    public static string ToTimestampString( this DateTime source )
    {

        var utc = source.ToUniversalTime();

        var y = utc.Year.ToString().PadLeft(4,'0');
        var m = utc.Month.ToString().PadLeft(2, '0');
        var d = utc.Day.ToString().PadLeft(2, '0');
        var t = utc.TimeOfDay.Ticks.ToString().PadLeft(20, '0');

        var ts = string.Join("", y, m, d, t);
        return ts;

    }

}