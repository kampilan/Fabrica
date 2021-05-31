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
using JetBrains.Annotations;

namespace Fabrica.Rules.Util
{

    internal static class Helpers
    {
        internal static int EncodeSignature( [NotNull] byte[] signatureIndices )
        {
            byte[] bytes = {0, 0, 0, 0};
            for( int i = 0; i < signatureIndices.Length; i++ )
                bytes[i] = (byte)(signatureIndices[i] + 1);

            int signature = BitConverter.ToInt32( bytes, 0 );

            return signature;
        }

        [NotNull]
        internal static byte[] DecodeSignature( int signature )
        {
            byte[] bytes = BitConverter.GetBytes( signature );

            int len = 0;
            for( int i = 0; i < 4; i++ )
                if( bytes[i] == 0 )
                {
                    len = i;
                    break;
                }

            var signatureIndices = new byte[len];
            for( int i = 0; i < len; i++ )
                signatureIndices[i] = (byte)(bytes[i] - 1);

            return signatureIndices;
        }


        internal static long EncodeSelector( [NotNull] int[] selectorIndices )
        {
            byte[] bytes = {0, 0, 0, 0, 0, 0, 0, 0};
            for( int i = 0; i < selectorIndices.Length; i++ )
            {
                byte[] b = BitConverter.GetBytes( (UInt16)selectorIndices[i] );
                Array.Copy( b, 0, bytes, (i*2), 2 );
            }

            long selector = BitConverter.ToInt64( bytes, 0 );

            return selector;
        }

        [NotNull]
        internal static int[] DecodeSelector( long selector )
        {
            byte[] bytes = BitConverter.GetBytes( selector );

            var full = new UInt16[4];

            full[0] = BitConverter.ToUInt16( bytes, 0 );
            full[1] = BitConverter.ToUInt16( bytes, 2 );
            full[2] = BitConverter.ToUInt16( bytes, 4 );
            full[3] = BitConverter.ToUInt16( bytes, 6 );

            int len = 4;
            for( int i = 0; i < 4; i++ )
                if( full[i] == 0 )
                {
                    len = i;
                    break;
                }

            var selectorIndices = new int[len];
            for( int i = 0; i < len; i++ )
                selectorIndices[i] = full[i];

            return selectorIndices;
        }
    }

}
