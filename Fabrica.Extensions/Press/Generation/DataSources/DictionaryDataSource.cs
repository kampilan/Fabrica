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


using System.Collections.Generic;

namespace Fabrica.Press.Generation.DataSources
{

    
    public class DictionaryDataSource: IMergeDataSource
    {


        public DictionaryDataSource( string region, IEnumerable<IDictionary<string, object>> data )
        {
            Region = region;
            List   = new List<IDictionary<string, object>>(data);
        }


        public DictionaryDataSource(string region, params IDictionary<string, object>[] data)
        {
            Region = region;
            List   = new List<IDictionary<string, object>>(data);
        }


        public string Region { get; }

        private int Index { get; set; } = -1;
        private List<IDictionary<string,object>> List { get; }


        public void Rewind()
        {
            Index = -1;
        }

        public IDictionary<string, object> Current => List[Index];


        public bool MoveNext()
        {

            Index++;

            if (Index >= List.Count)
                return false;

            return true;


        }

        public bool TryGetValue( string spec, out object value )
        {

            var ms = MergeField.Parse( spec );

            var found = List[Index].TryGetValue( ms.Name, out var str );
            value = found ? str : null;

            return found;

        }



    }



}
