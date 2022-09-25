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

using System.ComponentModel;
using Fabrica.Utilities.Text;
using Fabrica.Utilities.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.Work.Processor
{

    public class WorkRequest
    {



        private static JsonSerializerSettings BuildJsonSerializerSettings()
        {

            var settings = new JsonSerializerSettings
            {
                DateFormatHandling         = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling       = DateTimeZoneHandling.RoundtripKind,
                DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling          = NullValueHandling.Ignore,
                ReferenceLoopHandling      = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            return settings;

        }


        private static JsonSerializer BuildJsonSerializer()
        {

            var serializer = new JsonSerializer
            {
                DateFormatHandling         = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling       = DateTimeZoneHandling.RoundtripKind,
                DefaultValueHandling       = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling          = NullValueHandling.Ignore,
                ReferenceLoopHandling      = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            return serializer;

        }


        public static WorkRequest? FromJson(string json)
        {
            var settings = BuildJsonSerializerSettings();
            var request = JsonConvert.DeserializeObject<WorkRequest>(json, settings );
            return request;
        }


        public string Uid { get; set; } = Base62Converter.NewGuid();

        [DefaultValue("")]
        public string Topic { get; set; } = "";


        [DefaultValue(null)] 
        public JObject Payload { get; set; } = null!;


        public void ToPayload( object payload )
        {
            var serializer = BuildJsonSerializer();
            Payload = JObject.FromObject( payload, serializer );
        }

        public TPayload? FromPayload<TPayload>() where TPayload : class
        {
            var serializer = BuildJsonSerializer();
            var payload = Payload.ToObject<TPayload>( serializer );
            return payload;
        }


        public string ToJson()
        {
            var settings = BuildJsonSerializerSettings();
            var json = JsonConvert.SerializeObject(this, settings);
            return json;
        }


    }


}
