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
using System.Linq;
using JetBrains.Annotations;

namespace Fabrica.Rules.Validators
{

    public static class NumericValidatorEx
    {
        #region Char Support

        public static IValidator<TFact, char> Required<TFact>([NotNull] this IValidator<TFact, char> validator) where TFact : class
        {
            return validator.IsNot((f, v) => v == 0);
        }


        public static IValidator<TFact, char> IsNotZero<TFact>( [NotNull] this IValidator<TFact, char> validator ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v == 0 );
        }

        public static IValidator<TFact, char> IsZero<TFact>( [NotNull] this IValidator<TFact, char> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => v == 0 );
        }

        public static IValidator<TFact, char> IsEqual<TFact>( [NotNull] this IValidator<TFact, char> validator, char test ) where TFact : class
        {
            return validator.Is( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, char> IsEqual<TFact>([NotNull] this IValidator<TFact, char> validator, Func<TFact, char> extractor) where TFact : class
        {
            return validator.Is( ( f, v ) => extractor(f).CompareTo( v ) == 0 );
        }
       

        public static IValidator<TFact, char> IsNotEqual<TFact>( [NotNull] this IValidator<TFact, char> validator, char test ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, char> IsNotEqual<TFact>([NotNull] this IValidator<TFact, char> validator, Func<TFact, char> extractor) where TFact : class
        {
            return validator.IsNot((f, v) => extractor(f).CompareTo(v) == 0);
        }



        public static IValidator<TFact, char> IsGreaterThen<TFact>( [NotNull] this IValidator<TFact, char> validator, char test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v > test );
        }


        public static IValidator<TFact, char> IsGreaterThen<TFact>([NotNull] this IValidator<TFact, char> validator, Func<TFact, char> extractor) where TFact : class
        {
            return validator.Is((f, v) => v > extractor(f));
        }


        public static IValidator<TFact, char> IsLessThen<TFact>( [NotNull] this IValidator<TFact, char> validator, char test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v < test );
        }

        public static IValidator<TFact, char> IsLessThen<TFact>([NotNull] this IValidator<TFact, char> validator, Func<TFact, char> extractor) where TFact : class
        {
            return validator.Is((f, v) => v < extractor(f));
        }

        public static IValidator<TFact, char> IsBetween<TFact>([NotNull] this IValidator<TFact, char> validator, char low, char high) where TFact : class
        {
            return validator.Is((f, v) => v >= low && v <= high);
        }

        public static IValidator<TFact, char> IsNotBetween<TFact>( [NotNull] this IValidator<TFact, char> validator, char low, char high ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v >= low && v <= high );
        }


        public static IValidator<TFact, char> IsBetween<TFact>([NotNull] this IValidator<TFact, char> validator, Func<TFact, char> lowExtractor, Func<TFact, char> highExtractor ) where TFact : class
        {
            return validator.Is((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }

        public static IValidator<TFact, char> IsNotBetween<TFact>([NotNull] this IValidator<TFact, char> validator, Func<TFact, char> lowExtractor, Func<TFact, char> highExtractor ) where TFact : class
        {
            return validator.IsNot((f, v) => v >= lowExtractor(f) && v <= highExtractor(f) );
        }
        
        
        public static IValidator<TFact, char> IsIn<TFact>( [NotNull] this IValidator<TFact, char> validator, params char[] values ) where TFact : class
        {
            return validator.Is( ( f, v ) => values.Contains( v ) );
        }

        public static IValidator<TFact, char> IsNotIn<TFact>( [NotNull] this IValidator<TFact, char> validator, params char[] values ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => values.Contains( v ) );
        }

        #endregion

        #region Byte Support

        public static IValidator<TFact, byte> Required<TFact>([NotNull] this IValidator<TFact, byte> validator) where TFact : class
        {
            return validator.IsNot((f, v) => v == 0);
        }


        public static IValidator<TFact, byte> IsNotZero<TFact>( [NotNull] this IValidator<TFact, byte> validator ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v == 0 );
        }


        public static IValidator<TFact, byte> IsZero<TFact>( [NotNull] this IValidator<TFact, byte> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => v == 0 );
        }

        public static IValidator<TFact, byte> IsEqual<TFact>( [NotNull] this IValidator<TFact, byte> validator, byte test ) where TFact : class
        {
            return validator.Is( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, byte> IsEqual<TFact>([NotNull] this IValidator<TFact, byte> validator, Func<TFact, byte> extractor) where TFact : class
        {
            return validator.Is((f, v) => extractor(f).CompareTo(v) == 0);
        }



        public static IValidator<TFact, byte> IsNotEqual<TFact>( [NotNull] this IValidator<TFact, byte> validator, byte test ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, byte> IsNotEqual<TFact>([NotNull] this IValidator<TFact, byte> validator, Func<TFact, byte> extractor) where TFact : class
        {
            return validator.IsNot((f, v) => extractor(f).CompareTo(v) == 0);
        }



        public static IValidator<TFact, byte> IsGreaterThen<TFact>( [NotNull] this IValidator<TFact, byte> validator, byte test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v > test );
        }

        public static IValidator<TFact, byte> IsGreaterThen<TFact>([NotNull] this IValidator<TFact, byte> validator, Func<TFact, byte> extractor) where TFact : class
        {
            return validator.Is((f, v) => v > extractor(f));
        }



        public static IValidator<TFact, byte> IsLessThen<TFact>( [NotNull] this IValidator<TFact, byte> validator, byte test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v < test );
        }

        public static IValidator<TFact, byte> IsLessThen<TFact>([NotNull] this IValidator<TFact, byte> validator, Func<TFact, byte> extractor) where TFact : class
        {
            return validator.Is((f, v) => v < extractor(f));
        }



        public static IValidator<TFact, byte> IsBetween<TFact>( [NotNull] this IValidator<TFact, byte> validator, byte low, byte high ) where TFact : class
        {
            return validator.Is( ( f, v ) => v >= low && v <= high );
        }

        public static IValidator<TFact, byte> IsNotBetween<TFact>( [NotNull] this IValidator<TFact, byte> validator, byte low, byte high ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v >= low && v <= high );
        }


        public static IValidator<TFact, byte> IsBetween<TFact>([NotNull] this IValidator<TFact, byte> validator, Func<TFact, byte> lowExtractor, Func<TFact, byte> highExtractor) where TFact : class
        {
            return validator.Is((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }

        public static IValidator<TFact, byte> IsNotBetween<TFact>([NotNull] this IValidator<TFact, byte> validator, Func<TFact, byte> lowExtractor, Func<TFact, byte> highExtractor) where TFact : class
        {
            return validator.IsNot((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }



        public static IValidator<TFact, byte> IsIn<TFact>( [NotNull] this IValidator<TFact, byte> validator, params byte[] values ) where TFact : class
        {
            return validator.Is( ( f, v ) => values.Contains( v ) );
        }

        public static IValidator<TFact, byte> IsNotIn<TFact>( [NotNull] this IValidator<TFact, byte> validator, params byte[] values ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => values.Contains( v ) );
        }

        #endregion

        #region Short Support

        public static IValidator<TFact, short> Required<TFact>([NotNull] this IValidator<TFact, short> validator) where TFact : class
        {
            return validator.IsNot((f, v) => v == 0);
        }

        public static IValidator<TFact, short> IsNotZero<TFact>( [NotNull] this IValidator<TFact, short> validator ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v == 0 );
        }

        public static IValidator<TFact, short> IsZero<TFact>( [NotNull] this IValidator<TFact, short> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => v == 0 );
        }

        public static IValidator<TFact, short> IsEqual<TFact>( [NotNull] this IValidator<TFact, short> validator, short test ) where TFact : class
        {
            return validator.Is( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, short> IsEqual<TFact>([NotNull] this IValidator<TFact, short> validator, Func<TFact, short> extractor) where TFact : class
        {
            return validator.Is((f, v) => extractor(f).CompareTo(v) == 0);
        }




        public static IValidator<TFact, short> IsNotEqual<TFact>( [NotNull] this IValidator<TFact, short> validator, short test ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, short> IsNotEqual<TFact>([NotNull] this IValidator<TFact, short> validator, Func<TFact, short> extractor) where TFact : class
        {
            return validator.IsNot((f, v) => extractor(f).CompareTo(v) == 0);
        }



        public static IValidator<TFact, short> IsGreaterThen<TFact>( [NotNull] this IValidator<TFact, short> validator, short test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v > test );
        }

        public static IValidator<TFact, short> IsGreaterThen<TFact>([NotNull] this IValidator<TFact, short> validator, Func<TFact, short> extractor) where TFact : class
        {
            return validator.Is((f, v) => v > extractor(f));
        }



        public static IValidator<TFact, short> IsLessThen<TFact>( [NotNull] this IValidator<TFact, short> validator, short test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v < test );
        }

        public static IValidator<TFact, short> IsLessThen<TFact>([NotNull] this IValidator<TFact, short> validator, Func<TFact, short> extractor) where TFact : class
        {
            return validator.Is((f, v) => v < extractor(f));
        }



        public static IValidator<TFact, short> IsBetween<TFact>( [NotNull] this IValidator<TFact, short> validator, short low, short high ) where TFact : class
        {
            return validator.Is( ( f, v ) => v >= low && v <= high );
        }

        public static IValidator<TFact, short> IsNotBetween<TFact>( [NotNull] this IValidator<TFact, short> validator, short low, short high ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v >= low && v <= high );
        }


        public static IValidator<TFact, short> IsBetween<TFact>([NotNull] this IValidator<TFact, short> validator, Func<TFact, short> lowExtractor, Func<TFact, short> highExtractor) where TFact : class
        {
            return validator.Is((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }

        public static IValidator<TFact, short> IsNotBetween<TFact>([NotNull] this IValidator<TFact, short> validator, Func<TFact, short> lowExtractor, Func<TFact, short> highExtractor) where TFact : class
        {
            return validator.IsNot((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }


        public static IValidator<TFact, short> IsIn<TFact>( [NotNull] this IValidator<TFact, short> validator, params short[] values ) where TFact : class
        {
            return validator.Is( ( f, v ) => values.Contains( v ) );
        }

        public static IValidator<TFact, short> IsNotIn<TFact>( [NotNull] this IValidator<TFact, short> validator, params short[] values ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => values.Contains( v ) );
        }

        #endregion

        #region Int Support

        public static IValidator<TFact, int> Required<TFact>([NotNull] this IValidator<TFact, int> validator) where TFact : class
        {
            return validator.IsNot((f, v) => v == 0);
        }

        public static IValidator<TFact, int> IsNotZero<TFact>( [NotNull] this IValidator<TFact, int> validator ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v == 0 );
        }

        public static IValidator<TFact, int> IsZero<TFact>( [NotNull] this IValidator<TFact, int> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => v == 0 );
        }

        public static IValidator<TFact, int> IsEqual<TFact>( [NotNull] this IValidator<TFact, int> validator, int test ) where TFact : class
        {
            return validator.Is( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, int> IsEqual<TFact>([NotNull] this IValidator<TFact, int> validator, Func<TFact, int> extractor) where TFact : class
        {
            return validator.Is((f, v) => extractor(f).CompareTo(v) == 0);
        }



        public static IValidator<TFact, int> IsNotEqual<TFact>( [NotNull] this IValidator<TFact, int> validator, int test ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, int> IsNotEqual<TFact>([NotNull] this IValidator<TFact, int> validator, Func<TFact, int> extractor) where TFact : class
        {
            return validator.IsNot((f, v) => extractor(f).CompareTo(v) == 0);
        }



        public static IValidator<TFact, int> IsGreaterThen<TFact>( [NotNull] this IValidator<TFact, int> validator, int test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v > test );
        }

        public static IValidator<TFact, int> IsGreaterThen<TFact>([NotNull] this IValidator<TFact, int> validator, Func<TFact, int> extractor) where TFact : class
        {
            return validator.Is((f, v) => v > extractor(f));
        }



        public static IValidator<TFact, int> IsLessThen<TFact>( [NotNull] this IValidator<TFact, int> validator, int test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v < test );
        }

        public static IValidator<TFact, int> IsLessThen<TFact>([NotNull] this IValidator<TFact, int> validator, Func<TFact, int> extractor) where TFact : class
        {
            return validator.Is((f, v) => v < extractor(f));
        }



        public static IValidator<TFact, int> IsBetween<TFact>( [NotNull] this IValidator<TFact, int> validator, int low, int high ) where TFact : class
        {
            return validator.Is( ( f, v ) => v >= low && v <= high );
        }

        public static IValidator<TFact, int> IsNotBetween<TFact>( [NotNull] this IValidator<TFact, int> validator, int low, int high ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v >= low && v <= high );
        }


        public static IValidator<TFact, int> IsBetween<TFact>([NotNull] this IValidator<TFact, int> validator, Func<TFact, int> lowExtractor, Func<TFact, short> highExtractor) where TFact : class
        {
            return validator.Is((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }

        public static IValidator<TFact, int> IsNotBetween<TFact>([NotNull] this IValidator<TFact, int> validator, Func<TFact, int> lowExtractor, Func<TFact, int> highExtractor) where TFact : class
        {
            return validator.IsNot((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }



        public static IValidator<TFact, int> IsIn<TFact>( [NotNull] this IValidator<TFact, int> validator, params int[] values ) where TFact : class
        {
            return validator.Is( ( f, v ) => values.Contains( v ) );
        }

        public static IValidator<TFact, int> IsNotIn<TFact>( [NotNull] this IValidator<TFact, int> validator, params int[] values ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => values.Contains( v ) );
        }

        #endregion

        #region Long Support

        public static IValidator<TFact, long> Required<TFact>([NotNull] this IValidator<TFact, long> validator) where TFact : class
        {
            return validator.IsNot((f, v) => v == 0);
        }

        public static IValidator<TFact, long> IsNotZero<TFact>( [NotNull] this IValidator<TFact, long> validator ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v == 0 );
        }

        public static IValidator<TFact, long> IsZero<TFact>( [NotNull] this IValidator<TFact, long> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => v == 0 );
        }

        public static IValidator<TFact, long> IsEqual<TFact>( [NotNull] this IValidator<TFact, long> validator, long test ) where TFact : class
        {
            return validator.Is( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, long> IsEqual<TFact>([NotNull] this IValidator<TFact, long> validator, Func<TFact, long> extractor) where TFact : class
        {
            return validator.Is((f, v) => extractor(f).CompareTo(v) == 0);
        }



        public static IValidator<TFact, long> IsNotEqual<TFact>( [NotNull] this IValidator<TFact, long> validator, long test ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, long> IsNotEqual<TFact>([NotNull] this IValidator<TFact, long> validator, Func<TFact, long> extractor) where TFact : class
        {
            return validator.IsNot((f, v) => extractor(f).CompareTo(v) == 0);
        }



        public static IValidator<TFact, long> IsGreaterThen<TFact>( [NotNull] this IValidator<TFact, long> validator, long test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v > test );
        }

        public static IValidator<TFact, long> IsGreaterThen<TFact>([NotNull] this IValidator<TFact, long> validator, Func<TFact, long> extractor) where TFact : class
        {
            return validator.Is((f, v) => v > extractor(f));
        }



        public static IValidator<TFact, long> IsLessThen<TFact>( [NotNull] this IValidator<TFact, long> validator, long test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v < test );
        }

        public static IValidator<TFact, long> IsLessThen<TFact>([NotNull] this IValidator<TFact, long> validator, Func<TFact, long> extractor) where TFact : class
        {
            return validator.Is((f, v) => v < extractor(f));
        }



        public static IValidator<TFact, long> IsBetween<TFact>( [NotNull] this IValidator<TFact, long> validator, long low, long high ) where TFact : class
        {
            return validator.Is( ( f, v ) => v >= low && v <= high );
        }

        public static IValidator<TFact, long> IsNotBetween<TFact>( [NotNull] this IValidator<TFact, long> validator, long low, long high ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v >= low && v <= high );
        }

        public static IValidator<TFact, long> IsBetween<TFact>([NotNull] this IValidator<TFact, long> validator, Func<TFact, long> lowExtractor, Func<TFact, long> highExtractor) where TFact : class
        {
            return validator.Is((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }

        public static IValidator<TFact, long> IsNotBetween<TFact>([NotNull] this IValidator<TFact, long> validator, Func<TFact, long> lowExtractor, Func<TFact, long> highExtractor) where TFact : class
        {
            return validator.IsNot((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }



        public static IValidator<TFact, long> IsIn<TFact>( [NotNull] this IValidator<TFact, long> validator, params long[] values ) where TFact : class
        {
            return validator.Is( ( f, v ) => values.Contains( v ) );
        }

        public static IValidator<TFact, long> IsNotIn<TFact>( [NotNull] this IValidator<TFact, long> validator, params long[] values ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => values.Contains( v ) );
        }

        #endregion

        #region Float Support

        public static IValidator<TFact, float> Required<TFact>([NotNull] this IValidator<TFact, float> validator) where TFact : class
        {
            return validator.IsNot((f, v) => v.CompareTo(0f) == 0);
        }


        public static IValidator<TFact, float> IsNotZero<TFact>( [NotNull] this IValidator<TFact, float> validator ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v.CompareTo( 0f ) == 0 );
        }

        public static IValidator<TFact, float> IsZero<TFact>( [NotNull] this IValidator<TFact, float> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => v.CompareTo( 0f ) == 0 );
        }


        public static IValidator<TFact, float> IsEqual<TFact>( [NotNull] this IValidator<TFact, float> validator, float test ) where TFact : class
        {
            return validator.Is( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, float> IsEqual<TFact>([NotNull] this IValidator<TFact, float> validator, Func<TFact, float> extractor) where TFact : class
        {
            return validator.Is((f, v) => extractor(f).CompareTo(v) == 0);
        }



        public static IValidator<TFact, float> IsNotEqual<TFact>( [NotNull] this IValidator<TFact, float> validator, float test ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, float> IsNotEqual<TFact>([NotNull] this IValidator<TFact, float> validator, Func<TFact, float> extractor) where TFact : class
        {
            return validator.IsNot((f, v) => extractor(f).CompareTo(v) == 0);
        }



        public static IValidator<TFact, float> IsGreaterThen<TFact>( [NotNull] this IValidator<TFact, float> validator, float test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v > test );
        }

        public static IValidator<TFact, float> IsGreaterThen<TFact>([NotNull] this IValidator<TFact, float> validator, Func<TFact, float> extractor) where TFact : class
        {
            return validator.Is((f, v) => v > extractor(f));
        }



        public static IValidator<TFact, float> IsLessThen<TFact>( [NotNull] this IValidator<TFact, float> validator, float test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v < test );
        }

        public static IValidator<TFact, float> IsLessThen<TFact>([NotNull] this IValidator<TFact, float> validator, Func<TFact, float> extractor) where TFact : class
        {
            return validator.Is((f, v) => v < extractor(f));
        }




        public static IValidator<TFact, float> IsBetween<TFact>( [NotNull] this IValidator<TFact, float> validator, float low, float high ) where TFact : class
        {
            return validator.Is( ( f, v ) => v >= low && v <= high );
        }

        public static IValidator<TFact, float> IsNotBetween<TFact>( [NotNull] this IValidator<TFact, float> validator, float low, float high ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v >= low && v <= high );
        }

        public static IValidator<TFact, float> IsBetween<TFact>([NotNull] this IValidator<TFact, float> validator, Func<TFact, float> lowExtractor, Func<TFact, float> highExtractor) where TFact : class
        {
            return validator.Is((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }

        public static IValidator<TFact, float> IsNotBetween<TFact>([NotNull] this IValidator<TFact, float> validator, Func<TFact, float> lowExtractor, Func<TFact, float> highExtractor) where TFact : class
        {
            return validator.IsNot((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }



        public static IValidator<TFact, float> IsIn<TFact>( [NotNull] this IValidator<TFact, float> validator, params float[] values ) where TFact : class
        {
            return validator.Is( ( f, v ) => values.Contains( v ) );
        }

        public static IValidator<TFact, float> IsNotIn<TFact>( [NotNull] this IValidator<TFact, float> validator, params float[] values ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => values.Contains( v ) );
        }

        #endregion

        #region Double Support

        public static IValidator<TFact, double> Required<TFact>([NotNull] this IValidator<TFact, double> validator) where TFact : class
        {
            return validator.IsNot((f, v) => v.CompareTo(0d) == 0);
        }

        public static IValidator<TFact, double> IsNotZero<TFact>( [NotNull] this IValidator<TFact, double> validator ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v.CompareTo( 0d ) == 0 );
        }

        public static IValidator<TFact, double> IsZero<TFact>( [NotNull] this IValidator<TFact, double> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => v.CompareTo( 0d ) == 0 );
        }



        public static IValidator<TFact, double> IsEqual<TFact>( [NotNull] this IValidator<TFact, double> validator, double test ) where TFact : class
        {
            return validator.Is( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, double> IsEqual<TFact>([NotNull] this IValidator<TFact, double> validator, Func<TFact, double> extractor) where TFact : class
        {
            return validator.Is((f, v) => extractor(f).CompareTo(v) == 0);
        }



        public static IValidator<TFact, double> IsNotEqual<TFact>( [NotNull] this IValidator<TFact, double> validator, double test ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, double> IsNotEqual<TFact>([NotNull] this IValidator<TFact, double> validator, Func<TFact, double> extractor) where TFact : class
        {
            return validator.IsNot((f, v) => extractor(f).CompareTo(v) == 0);
        }



        public static IValidator<TFact, double> IsGreaterThen<TFact>( [NotNull] this IValidator<TFact, double> validator, double test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v > test );
        }

        public static IValidator<TFact, double> IsGreaterThen<TFact>([NotNull] this IValidator<TFact, double> validator, Func<TFact, double> extractor) where TFact : class
        {
            return validator.Is((f, v) => v > extractor(f));
        }




        public static IValidator<TFact, double> IsLessThen<TFact>( [NotNull] this IValidator<TFact, double> validator, double test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v < test );
        }

        public static IValidator<TFact, double> IsLessThen<TFact>([NotNull] this IValidator<TFact, double> validator, Func<TFact, double> extractor) where TFact : class
        {
            return validator.Is((f, v) => v < extractor(f));
        }




        public static IValidator<TFact, double> IsBetween<TFact>( [NotNull] this IValidator<TFact, double> validator, double low, double high ) where TFact : class
        {
            return validator.Is( ( f, v ) => v >= low && v <= high );
        }

        public static IValidator<TFact, double> IsNotBetween<TFact>( [NotNull] this IValidator<TFact, double> validator, double low, double high ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v >= low && v <= high );
        }

        public static IValidator<TFact, double> IsBetween<TFact>([NotNull] this IValidator<TFact, double> validator, Func<TFact, double> lowExtractor, Func<TFact, double> highExtractor) where TFact : class
        {
            return validator.Is((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }

        public static IValidator<TFact, double> IsNotBetween<TFact>([NotNull] this IValidator<TFact, double> validator, Func<TFact, double> lowExtractor, Func<TFact, double> highExtractor) where TFact : class
        {
            return validator.IsNot((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }



        public static IValidator<TFact, double> IsIn<TFact>( [NotNull] this IValidator<TFact, double> validator, params double[] values ) where TFact : class
        {
            return validator.Is( ( f, v ) => values.Contains( v ) );
        }

        public static IValidator<TFact, double> IsNotIn<TFact>( [NotNull] this IValidator<TFact, double> validator, params double[] values ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => values.Contains( v ) );
        }

        #endregion

        #region Decimal Support

        public static IValidator<TFact, decimal> Required<TFact>([NotNull] this IValidator<TFact, decimal> validator) where TFact : class
        {
            return validator.IsNot((f, v) => v.CompareTo(decimal.Zero) == 0);
        }

        public static IValidator<TFact, decimal> IsNotZero<TFact>( [NotNull] this IValidator<TFact, decimal> validator ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v.CompareTo( decimal.Zero ) == 0 );
        }

        public static IValidator<TFact, decimal> IsZero<TFact>( [NotNull] this IValidator<TFact, decimal> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => v.CompareTo( decimal.Zero ) == 0 );
        }

        public static IValidator<TFact, decimal> IsEqual<TFact>( [NotNull] this IValidator<TFact, decimal> validator, decimal test ) where TFact : class
        {
            return validator.Is( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, decimal> IsEqual<TFact>([NotNull] this IValidator<TFact, decimal> validator, Func<TFact, decimal> extractor) where TFact : class
        {
            return validator.Is((f, v) => extractor(f).CompareTo(v) == 0);
        }


        public static IValidator<TFact, decimal> IsNotEqual<TFact>( [NotNull] this IValidator<TFact, decimal> validator, decimal test ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, decimal> IsNotEqual<TFact>([NotNull] this IValidator<TFact, decimal> validator, Func<TFact, decimal> extractor) where TFact : class
        {
            return validator.IsNot((f, v) => extractor(f).CompareTo(v) == 0);
        }


        public static IValidator<TFact, decimal> IsGreaterThen<TFact>( [NotNull] this IValidator<TFact, decimal> validator, decimal test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v > test );
        }

        public static IValidator<TFact, decimal> IsGreaterThen<TFact>([NotNull] this IValidator<TFact, decimal> validator, Func<TFact, decimal> extractor) where TFact : class
        {
            return validator.Is((f, v) => v > extractor(f));
        }


        public static IValidator<TFact, decimal> IsLessThen<TFact>( [NotNull] this IValidator<TFact, decimal> validator, decimal test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v < test );
        }

        public static IValidator<TFact, decimal> IsLessThen<TFact>([NotNull] this IValidator<TFact, decimal> validator, Func<TFact, decimal> extractor) where TFact : class
        {
            return validator.Is((f, v) => v < extractor(f));
        }



        public static IValidator<TFact, decimal> IsBetween<TFact>( [NotNull] this IValidator<TFact, decimal> validator, decimal low, decimal high ) where TFact : class
        {
            return validator.Is( ( f, v ) => v >= low && v <= high );
        }

        public static IValidator<TFact, decimal> IsNotBetween<TFact>( [NotNull] this IValidator<TFact, decimal> validator, decimal low, decimal high ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => v >= low && v <= high );
        }

        public static IValidator<TFact, decimal> IsBetween<TFact>([NotNull] this IValidator<TFact, decimal> validator, Func<TFact, decimal> lowExtractor, Func<TFact, decimal> highExtractor) where TFact : class
        {
            return validator.Is((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }

        public static IValidator<TFact, decimal> IsNotBetween<TFact>([NotNull] this IValidator<TFact, decimal> validator, Func<TFact, decimal> lowExtractor, Func<TFact, decimal> highExtractor) where TFact : class
        {
            return validator.IsNot((f, v) => v >= lowExtractor(f) && v <= highExtractor(f));
        }



        public static IValidator<TFact, decimal> IsIn<TFact>( [NotNull] this IValidator<TFact, decimal> validator, params decimal[] values ) where TFact : class
        {
            return validator.Is( ( f, v ) => values.Contains( v ) );
        }

        public static IValidator<TFact, decimal> IsNotIn<TFact>( [NotNull] this IValidator<TFact, decimal> validator, params decimal[] values ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => values.Contains( v ) );
        }

        #endregion
    }

}
