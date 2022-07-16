/*
The MIT License (MIT)

Copyright (c) 2021 The Kampilan Group Inc.

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

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Fabrica.Watch;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Fabrica.Http
{


    public class HttpRequest
    {

        public string HttpClientName { get; set; } = "Api";

        public string Path { get; set; }

        public HttpMethod Method { get; set; } = HttpMethod.Get;


        public IDictionary<string, string> CustomHeaders { get; } = new Dictionary<string, string>();


        public HttpContent BodyContent { get; set; }


        public bool DebugMode { get; set; }


        [NotNull]
        protected virtual JsonSerializerSettings GetSerializerSettings([CanBeNull] IContractResolver resolver = null)
        {

            var settings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DefaultValueHandling = DefaultValueHandling.Populate,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateParseHandling = DateParseHandling.DateTime,
                Formatting = Formatting.None,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            if (resolver != null)
                settings.ContractResolver = resolver;


            return settings;

        }


        public void ToBody([NotNull] object model, [CanBeNull] IContractResolver resolver = null)
        {

            if (model == null) throw new ArgumentNullException(nameof(model));

            using var logger = this.EnterMethod();


            var settings = GetSerializerSettings(resolver);
            var json = JsonConvert.SerializeObject(model, settings);

            ToBody(json);

        }


        public void ToBody([NotNull] string json )
        {

            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(json));


            using var logger = this.EnterMethod();

            logger.Inspect(nameof(json), json);

            BodyContent = new StringContent(json, Encoding.UTF8, "application/json");

        }


    }



}
