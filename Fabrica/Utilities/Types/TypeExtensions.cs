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
using System.Globalization;
using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;

namespace Fabrica.Utilities.Types
{

    public static class TypeExtensions
    {

        public static T As<T>([NotNull] this object value)
        {

            if (value.GetType().IsAssignableFrom(typeof(T)))
                return (T)value;

            if (value is IConvertible convertible)
                return (T)convertible.ToType(typeof(T), CultureInfo.CurrentCulture );

            return (T)value;

        }


        [NotNull]
        public static dynamic AsExpando<TTarget>( this TTarget value  ) where TTarget: class
        {
            return new ExpandoWrapper<TTarget>( value );
        }


        public static object As([NotNull] this object value, [NotNull] Type type )
        {

            if (type == null) throw new ArgumentNullException(nameof(type));

            if (value.GetType().IsAssignableFrom(type))
                return value;

            if (value is IConvertible convertible)
                return convertible.ToType(type, CultureInfo.CurrentCulture );

            return value;

        }


        [NotNull]
        public static T Copy<T>( this object source ) where T : new()
        {

            var target = new T();

            Copy( source, target, new HashSet<string>() );

            return target;

        }


        public static void Copy( this object source, [NotNull] object target )
        {

            if (target == null) throw new ArgumentNullException(nameof(target));

            Copy(source, target, new HashSet<string>());
        }


        [NotNull]
        public static T Copy<T>( this object source, [NotNull] ISet<string> ignore ) where T : new()
        {

            if (ignore == null) throw new ArgumentNullException(nameof(ignore));

            var target = new T();

            Copy(source, target, ignore );

            return target;

        }


        public static void Copy( this object source, [NotNull] object target, [NotNull] ISet<string> ignore )
        {

            if (target == null) throw new ArgumentNullException(nameof(target));
            if (ignore == null) throw new ArgumentNullException(nameof(ignore));

            foreach (var prop in target.GetType().GetProperties() )
            {

                if (!prop.CanWrite)
                    continue;

                var fromProp = source.GetType().GetProperty(prop.Name);
                if ((fromProp == null) || !(fromProp.CanRead))
                    continue;

                if ( ignore.Contains( fromProp.Name ) )
                    continue;

                var oValue = fromProp.GetValue(source, null);
                prop.SetValue(target, oValue, null);

            }
        }


        public static string ToGZip( this string source, CompressionLevel level = CompressionLevel.Optimal )
        {

            string b64;
            using (var input = new MemoryStream())
            {

                using (var writer = new StreamWriter(input))
                {


                    // *****************************************************
                    writer.Write(source);
                    writer.Flush();

                    input.Seek(0, SeekOrigin.Begin);



                    // *****************************************************
                    using( var output = new MemoryStream() )
                    {

                        // *****************************************************                        
                        using( var gzip = new GZipStream( output, CompressionLevel.Optimal, true ) )
                        {
                            input.CopyTo( gzip );
                            gzip.Flush();
                        }

                        output.Seek(0, SeekOrigin.Begin);



                        // *****************************************************
                        b64 = Convert.ToBase64String(output.ToArray());


                    }

                }

            }

            return b64;
            
        }


        public static string FromGZip(this string source)
        {


            // *****************************************************
            var buf = Convert.FromBase64String(source);


            
            // *****************************************************
            string clear;
            using (var input = new MemoryStream(buf))
            {


                // *****************************************************
                using( var output = new MemoryStream() )
                {

                    // *****************************************************
                    using( var gzip = new GZipStream( input, CompressionMode.Decompress ) )
                    {
                        gzip.CopyTo( output );
                        gzip.Flush();
                    }

                    output.Seek( 0, SeekOrigin.Begin );



                    // *****************************************************
                    using( var reader = new StreamReader( output ) )
                        clear = reader.ReadToEnd();


                }


            }



            // *****************************************************
            return clear;

        }


        public static string ToHexString( [NotNull] this byte[] bytes )
        {

            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            var hex = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            return hex;

        }


    }

}