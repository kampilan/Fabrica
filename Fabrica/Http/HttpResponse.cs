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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Fabrica.Http
{
 
    
    public class HttpResponse
    {

        public HttpResponse(HttpStatusCode statusCode, string reasonPhrase, bool wasSuccessful, string content = "")
        {
            StatusCode    = statusCode;
            ReasonPhrase  = reasonPhrase;
            WasSuccessful = wasSuccessful;
            Content       = content;
        }

        
        public HttpStatusCode StatusCode { get;  }

        public string ReasonPhrase { get; }


        public bool WasSuccessful { get; }


        public bool HasContent => !string.IsNullOrWhiteSpace(Content);


        public string Content { get; set; }


        [NotNull]
        protected virtual JsonSerializer GetSerializer([CanBeNull] IContractResolver resolver = null)
        {

            var settings = new JsonSerializer
            {
                DateTimeZoneHandling       = DateTimeZoneHandling.Local,
                DefaultValueHandling       = DefaultValueHandling.Ignore,
                DateFormatHandling         = DateFormatHandling.IsoDateFormat,
                DateParseHandling          = DateParseHandling.DateTime,
                ContractResolver           = new DefaultContractResolver(),
                Formatting                 = Formatting.None,
                MissingMemberHandling      = MissingMemberHandling.Ignore,
                NullValueHandling          = NullValueHandling.Ignore,
                ObjectCreationHandling     = ObjectCreationHandling.Auto,
                ReferenceLoopHandling      = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            if (resolver != null)
                settings.ContractResolver = resolver;


            return settings;

        }



        public TModel FromBody<TModel>( [CanBeNull] IContractResolver resolver = null ) where TModel: class
        {

            var serializer = GetSerializer(resolver);

            using( var reader = new StringReader(HasContent ? Content : "{}") )
            using( var jreader = new JsonTextReader(reader) )
            {
                var model = serializer.Deserialize<TModel>(jreader);
                return model;
            }

        }


        public object FromBody( [NotNull] Type modelType, [CanBeNull] IContractResolver resolver = null )
        {

            var serializer = GetSerializer(resolver);

            using( var reader = new StringReader(HasContent ? Content : "{}") )
            using( var jreader = new JsonTextReader(reader) )
            {
                var model = serializer.Deserialize(jreader, modelType);
                return model;
            }


        }


        public List<TType> FromBodyToList<TType>( [CanBeNull] IContractResolver resolver = null ) where TType: class
        {

            var serializer = GetSerializer(resolver);

            var array = JArray.Parse( HasContent ? Content : "[]" );

            var list = array.Select( token => token.ToObject<TType>( serializer ) ).ToList();

            return list;

        }


    }


}
