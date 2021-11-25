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
using System.ComponentModel;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedMember.Global
namespace Fabrica.Press.Generation.DataSources
{


    public class JsonDataSource: IMergeDataSource
    {


        [DefaultValue("")]
        public string Region { get; set; } = "";

        public JToken Data { get; set; }


        #region IMailMergeDataSource implementation

        public void Rewind()
        {
            Index = -1;
        }

        private int     Index   { get; set; } = -1;
        private JObject Current { get; set; }

        public bool MoveNext()
        {

            if( Data == null )
                throw new Exception( "Invalid Data implementation.Must not be null" );

            if( Data.Type == JTokenType.Object && Index == 0 )
                return false;

            if( Data.Type == JTokenType.Object )
            {
                Current = (JObject)Data;
                Index = 0;
                return true;
            }

            if( Data.Type != JTokenType.Array )
                throw new Exception( $"Invalid Data implementation: ({Data.Type}). Must be Object or Array" );

            var list = (JArray) Data;


            Index++;
            if (Index >= list.Count)
                return false;

            Current = (JObject)list[Index];


            return true;

        }


        public bool TryGetValue( string spec, out object value )
        {

            var ms = MergeField.Parse(spec);


            var prop = Current.Property(ms.Name);
            if( prop == null )
            {
                value = null;
                return false;
            }


            var jv = (JValue)prop.Value;

            switch (jv.Type)
            {
                case JTokenType.Float:
                    value = (decimal)jv;
                    break;
                case JTokenType.Integer:
                    value = (long)jv;
                    break;
                case JTokenType.String:
                    value = (string)jv;
                    break;
                case JTokenType.Boolean:
                    value = (bool)jv;
                    break;
                case JTokenType.Date:
                    value = (DateTime)jv;
                    break;
                case JTokenType.TimeSpan:
                    value = (TimeSpan)jv;
                    break;
                default:
                    value = null;
                    break;
            }

            return true;

        }


        #endregion


    }

}