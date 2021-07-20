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


using System.IO;
using GemBox.Document;

namespace Fabrica.Press.Generation.Transformers
{


    public class PictureTransformer: IInlineTransformer
    {


        public enum FormatType { Png, Jpeg, Tiff }


        public FormatType Format { get; set; } = FormatType.Png;

        public double Height { get; set; } = 0;
        public double Width  { get; set; } = 0;

        public MemoryStream Content { get; } = new MemoryStream();


        public Inline Handle( Inline target )
        {


            PictureFormat format;
            switch( Format )
            {
                case FormatType.Png:
                    format = PictureFormat.Png;
                    break;
                case FormatType.Jpeg:
                    format = PictureFormat.Jpeg;
                    break;
                case FormatType.Tiff:
                    format = PictureFormat.Tiff;
                    break;
                default:
                    format = PictureFormat.Png;
                    break;
            }

            var pict = new Picture( target.Document, Content, format, Width, Height, LengthUnit.Pixel );

            return pict;


        }



    }


}
