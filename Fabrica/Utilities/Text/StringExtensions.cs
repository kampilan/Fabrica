
// ReSharper disable UnusedMember.Global

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

using System.Text.RegularExpressions;

namespace Fabrica.Utilities.Text
{

    public static partial class StringExtensions
    {

        public static bool IsBase64String(this string s, bool relaxed=false)
        {
            s = s.Trim();
            return (relaxed ||s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

        }

        public static bool IsBase64UrlSafe(this string s)
        {
            s = s.Trim();
            return Base64Regex().IsMatch(s);
        }

        private static char[] Punctuation => " \t`~!@#$%^&*()-_=+\\/?:;.,".ToCharArray();
        public static string ToClean( this string self, params char[] allowedCharacters )
        {
            var chars = self.Where(c => char.IsLetterOrDigit(c) || Punctuation.Contains(c) || allowedCharacters.Contains(c) ).ToArray();
            return new string( chars );
        }

        [GeneratedRegex("^[a-zA-Z0-9-_]", RegexOptions.None)]
        private static partial Regex Base64Regex();
    }


}
