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

namespace Fabrica.Press.Generation.Formatters
{


    public class PadFormatter: IMergeFieldFormatter
    {

        public enum AlignmentType { Right, Left, Center }

        public PadFormatter( string tag, AlignmentType alignment, int length )
        {
            Tag       = tag;
            Alignment = alignment;
            Length    = length;
        }


        private AlignmentType Alignment { get; }
        private int Length { get; }



        #region IMergeFieldFormatter implementation

        public string Tag { get;  }
        public object Render( object value )
        {

            var str = value.ToString();
            var len = str.Length;

            var formatted = str;
            if (Alignment == AlignmentType.Right)
                formatted = str.PadRight(Length);
            else if (Alignment == AlignmentType.Left)
                formatted = str.PadLeft(Length);
            else if (Alignment == AlignmentType.Center)
                formatted = str.PadLeft((Length - len) / 2 + len).PadRight(Length);

            return formatted;

        }

        #endregion


    }


}
