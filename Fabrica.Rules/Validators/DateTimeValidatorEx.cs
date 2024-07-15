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


namespace Fabrica.Rules.Validators
{

    public static class DateTimeValidatorEx
    {

        public static IValidator<TFact, DateTime> Required<TFact>( this IValidator<TFact, DateTime> validator) where TFact : class
        {
            return validator.Is((f, v) => (v != DateTime.MinValue) && (v != DateTime.MaxValue) );
        }

        public static IValidator<TFact, DateTime> IsEqualTo<TFact>(  this IValidator<TFact, DateTime> validator, DateTime test ) where TFact : class
        {
            return validator.Is( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, DateTime> IsEqualTo<TFact>( this IValidator<TFact, DateTime> validator, Func<TFact,DateTime> extractor) where TFact : class
        {
            return validator.Is((f, v) => extractor(f).CompareTo(v) == 0);
        }


        public static IValidator<TFact, DateTime> IsNotEqualTo<TFact>(  this IValidator<TFact, DateTime> validator, DateTime test ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => test.CompareTo( v ) == 0 );
        }

        public static IValidator<TFact, DateTime> IsNotEqualTo<TFact>( this IValidator<TFact, DateTime> validator, Func<TFact,DateTime> extractor ) where TFact : class
        {
            return validator.IsNot((f, v) => extractor(f).CompareTo(v) == 0);
        }


        public static IValidator<TFact, DateTime> IsGreaterThen<TFact>(  this IValidator<TFact, DateTime> validator, DateTime test ) where TFact : class
        {
            return validator.Is( ( f, v ) => test.CompareTo( v ) == -1 );
        }

        public static IValidator<TFact, DateTime> IsGreaterThen<TFact>( this IValidator<TFact, DateTime> validator, Func<TFact, DateTime> extractor ) where TFact : class
        {
            return validator.Is((f, v) => extractor(f).CompareTo(v) == -1);
        }


        public static IValidator<TFact, DateTime> IsLessThen<TFact>(  this IValidator<TFact, DateTime> validator, DateTime test ) where TFact : class
        {
            return validator.Is( ( f, v ) => test.CompareTo( v ) == 1 );
        }

        public static IValidator<TFact, DateTime> IsLessThen<TFact>( this IValidator<TFact, DateTime> validator, Func<TFact, DateTime> extractor ) where TFact : class
        {
            return validator.Is((f, v) => extractor(f).CompareTo(v) == 1);
        }


        public static IValidator<TFact, DateTime> IsBetween<TFact>(  this IValidator<TFact, DateTime> validator, DateTime low, DateTime high ) where TFact : class
        {
            return validator.Is( ( f, v ) => (v >= low) && (v <= high) );
        }

        public static IValidator<TFact, DateTime> IsBetween<TFact>( this IValidator<TFact, DateTime> validator, Func<TFact, DateTime> lowExtractor, Func<TFact, DateTime> highExtractor ) where TFact : class
        {
            return validator.Is((f, v) => (v >= lowExtractor(f)) && (v <= highExtractor(f) ));
        }


        public static IValidator<TFact, DateTime> IsNotBetween<TFact>(  this IValidator<TFact, DateTime> validator, DateTime low, DateTime high ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => (v >= low) && (v <= high) );
        }

        public static IValidator<TFact, DateTime> IsNotBetween<TFact>( this IValidator<TFact, DateTime> validator, Func<TFact, DateTime> lowExtractor, Func<TFact, DateTime> highExtractor) where TFact : class
        {
            return validator.IsNot((f, v) => (v >= lowExtractor(f)) && (v <= highExtractor(f) ));
        }



        public static IValidator<TFact, DateTime> IsDayOfWeek<TFact>(  this IValidator<TFact, DateTime> validator, DayOfWeek test ) where TFact : class
        {
            return validator.Is( ( f, v ) => v.DayOfWeek == test );
        }

        public static IValidator<TFact, DateTime> IsWeekend<TFact>(  this IValidator<TFact, DateTime> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => (v.DayOfWeek == DayOfWeek.Saturday) || (v.DayOfWeek == DayOfWeek.Sunday) );
        }

        public static IValidator<TFact, DateTime> IsWeekday<TFact>(  this IValidator<TFact, DateTime> validator ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => (v.DayOfWeek == DayOfWeek.Saturday) || (v.DayOfWeek == DayOfWeek.Sunday) );
        }

        public static IValidator<TFact, DateTime> IsInFuture<TFact>(  this IValidator<TFact, DateTime> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => v > DateTime.Now );
        }

        public static IValidator<TFact, DateTime> IsInPast<TFact>(  this IValidator<TFact, DateTime> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => v <= DateTime.Now );
        }

        public static IValidator<TFact, DateTime> IsDaysInFuture<TFact>(  this IValidator<TFact, DateTime> validator, int days ) where TFact : class
        {
            return validator.Is( ( f, v ) => (v - DateTime.Now).TotalDays >= days );
        }

        public static IValidator<TFact, DateTime> IsDaysInPast<TFact>(  this IValidator<TFact, DateTime> validator, int days ) where TFact : class
        {
            return validator.Is( ( f, v ) => (DateTime.Now - v).TotalDays >= days );
        }

    }

}
