﻿/*
The MIT License (MIT)

Copyright © 2012-2019 Rick Strahl, West Wind Technologies

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

using System.Collections.Specialized;

namespace Fabrica.Utilities.Text
{

    /// <summary>
    /// A query string or UrlEncoded form parser and editor 
    /// class that allows reading and writing of urlencoded
    /// key value pairs used for query string and HTTP 
    /// form data.
    /// 
    /// Useful for parsing and editing querystrings inside
    /// of non-Web code that doesn't have easy access to
    /// the HttpUtility class.                
    /// </summary>
    /// <remarks>
    /// Supports multiple values per key
    /// </remarks>
    public class UrlEncodingParser : NameValueCollection
    {

        /// <summary>
        /// Holds the original Url that was assigned if any
        /// Url must contain // to be considered a url
        /// </summary>
        private string Url { get; set; }

        /// <summary>
        /// Determines whether plus signs in the UrlEncoded content
        /// are treated as spaces.
        /// </summary>
        public bool DecodePlusSignsAsSpaces { get; set; }

        /// <summary>
        /// Always pass in a UrlEncoded data or a URL to parse from
        /// unless you are creating a new one from scratch.
        /// </summary>
        /// <param name="queryStringOrUrl">
        /// Pass a query string or raw Form data, or a full URL.
        /// If a URL is parsed the part prior to the ? is stripped
        /// but saved. Then when you write the original URL is 
        /// re-written with the new query string.
        /// </param>
        /// <param name="decodeSpacesAsPlusSigns"></param>
        public UrlEncodingParser( string? queryStringOrUrl = null, bool decodeSpacesAsPlusSigns = false)
        {
            Url = string.Empty;
            DecodePlusSignsAsSpaces = decodeSpacesAsPlusSigns;
            if (!string.IsNullOrEmpty(queryStringOrUrl))
            {
                Parse(queryStringOrUrl);
            }
        }


        /// <summary>
        /// Assigns multiple values to the same key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public void SetValues( string key, IEnumerable<string> values )
        {
            foreach (var val in values)
                Add(key, val);
        }

        /// <summary>
        /// Parses the query string into the internal dictionary
        /// and optionally also returns this dictionary
        /// </summary>
        /// <param name="query">
        /// Query string key value pairs or a full URL. If URL is
        /// passed the URL is re-written in Write operation
        /// </param>
        /// <returns></returns>
        public NameValueCollection Parse(string query)
        {
            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
                Url = query;

            if (string.IsNullOrEmpty(query))
                Clear();
            else
            {
                int index = query.IndexOf('?');
                if (index > -1)
                {
                    if (query.Length >= index + 1)
                        query = query.Substring(index + 1);
                }

                var pairs = query.Split('&');
                foreach (var pair in pairs)
                {
                    int index2 = pair.IndexOf('=');
                    if (index2 > 0)
                    {
                        var val = pair.Substring(index2 + 1);
                        if (!string.IsNullOrEmpty(val))
                        {
                            if (DecodePlusSignsAsSpaces)
                                val = val.Replace("+", " ");
                            val = Uri.UnescapeDataString(val);
                        }

                        Add(pair.Substring(0, index2), val);
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Writes out the urlencoded data/query string or full URL based 
        /// on the internally set values.
        /// </summary>
        /// <returns>urlencoded data or url</returns>
        public override string ToString()
        {
            var query = string.Empty;
            foreach (string key in Keys)
            {
                var values = GetValues(key);
                if (values == null)
                    continue;

                foreach (var val in values)
                {
                    query += key + "=" + Uri.EscapeDataString(val) + "&";
                }
            }
            query = query.Trim('&');

            if (!string.IsNullOrEmpty(Url))
            {
                if (Url.Contains("?"))
                    query = Url.Substring(0, Url.IndexOf('?') + 1) + query;
                else
                    query = Url + "?" + query;
            }

            return query;
        }
    }
}