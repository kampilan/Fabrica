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

using System.Security.Cryptography;

namespace Fabrica.Utilities.Text
{

    public static class CodeGenerator
    {

        private static RandomNumberGenerator Rng { get; } = RandomNumberGenerator.Create();


        public static string Base36( int length )
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var code = new string( Enumerable.Repeat( chars, length ).Select( s => s[random.Next( s.Length )] ).ToArray() );
            return code;
        }


        public static string BaseX( string chars, byte[] random )
        {
            if (chars == null) throw new ArgumentNullException(nameof(chars));
            if (random == null) throw new ArgumentNullException(nameof(random));

            var buf = new char[random.Length];
            for(var i = 0; i < random.Length; i++)
            {
                var ix = random[i]%chars.Length;
                buf[i] = chars[ix];
            }


            var code = new string( buf );

            return code;
        }


        public static string Base32( int length )
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            var code = new string( Enumerable.Repeat( chars, length ).Select( s => s[random.Next( s.Length )] ).ToArray() );
            return code;
        }


        public static string Base32( byte[] random )
        {

            if (random == null) throw new ArgumentNullException(nameof(random));

            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var code = BaseX( chars, random );
            return code;
        }


        public static string Base32Clean( int length )
        {
            const string chars = "BBCDDFGHJKLMNPQRSTTVWXXZ23456789";
            var random = new Random();
            var code = new string( Enumerable.Repeat( chars, length ).Select( s => s[random.Next( s.Length )] ).ToArray() );
            return code;
        }


        public static string Base32Clean( byte[] random )
        {

            if (random == null) throw new ArgumentNullException(nameof(random));

            const string chars = "BBCDDFGHJKLMNPQRSTTVWXXZ23456789";
            var code = BaseX( chars, random );
            return code;
        }


        public static string Base62( int length )
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var code = new string( Enumerable.Repeat( chars, length ).Select( s => s[random.Next( s.Length )] ).ToArray() );
            return code;
        }


        public static string Base62( byte[] random )
        {

            if (random == null) throw new ArgumentNullException(nameof(random));

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var code = BaseX( chars, random );
            return code;
        }


        public static string Base62Clean( int length )
        {
            const string chars = "BBCDDFGHHJKLMNNPQRSTTVWXXZzbcdffghjjklmnppqrstvvwxzz0123456789";
            var random = new Random();
            var code = new string( Enumerable.Repeat( chars, length ).Select( s => s[random.Next( s.Length )] ).ToArray() );
            return code;
        }


        public static string Base62Clean( byte[] random )
        {

            if (random == null) throw new ArgumentNullException(nameof(random));

            const string chars = "BBCDDFGHHJKLMNNPQRSTTVWXXZzbcdffghjjklmnppqrstvvwxzz0123456789";
            var code = BaseX( chars, random );
            return code;

        }


        public static string UniqueRandom()
        {

            var guid = Guid.NewGuid();
            var rnd  = new byte[8];
            Rng.GetNonZeroBytes(rnd);


            var buf = new byte[24];

            Buffer.BlockCopy( guid.ToByteArray(),0, buf, 0, 16);
            Buffer.BlockCopy( rnd, 0, buf, 16, 8 );


            var code = Convert.ToBase64String(buf)
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=","");
            
            return code;

        }


    }

}