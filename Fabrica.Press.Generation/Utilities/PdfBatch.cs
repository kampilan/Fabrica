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
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Fabrica.Press.Utilities
{


    public class PdfBatch : IDisposable
    {


        public static implicit operator MemoryStream( PdfBatch batch )
        {

            var stream = new MemoryStream();

            batch.Content.Save( stream, false );

            stream.Seek(0, SeekOrigin.Begin);
            return stream;

        }


        public static implicit operator Stream( PdfBatch batch )
        {

            var stream = new MemoryStream();

            batch.Content.Save(stream, false);

            stream.Seek(0, SeekOrigin.Begin);
            return stream;

        }



        public PdfBatch()
        {
            Content = new PdfDocument();
        }


        private PdfDocument Content { get; }


        public int Add( Stream source )
        {

            var count = 0;
            using( var pdf = PdfReader.Open( source, PdfDocumentOpenMode.Import ) )
            {

                for( var i=0; i<pdf.Pages.Count; i++ )
                {
                    Content.AddPage(pdf.Pages[i]);
                    count++;
                }

            }

            return count;

        }


        public void Save( Stream stream )
        {
            Content.Save( stream, false );
        }


        public void Dispose()
        {
            Content?.Dispose();
        }


    }


}
