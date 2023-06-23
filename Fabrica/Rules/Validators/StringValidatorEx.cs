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

using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Fabrica.Rules.Validators
{

    public static class StringValidatorEx
    {


        static StringValidatorEx()
        {

            _USStates = new HashSet<string>( new[] { "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY", "DC" } );

            _states = new HashSet<string>( new[] { "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD", "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY", "DC", "PR", "VI", "AS", "GU", "MP", "AB", "BC", "MB", "NB", "NL", "NS", "ON", "PE", "QC", "SK", "NT", "NU", "YT" } );


        }

        public static IValidator<TFact, object> Required<TFact>(this IValidator<TFact, object> validator) where TFact : class
        {
            return validator.Is((f, v) => v is not null );
        }


        public static IValidator<TFact, string> Required<TFact>( this IValidator<TFact, string> validator) where TFact : class
        {
            return validator.Is((f, v) => !(string.IsNullOrWhiteSpace(v)));
        }


        public static IValidator<TFact, string> IsEmpty<TFact>( this IValidator<TFact, string> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => string.IsNullOrWhiteSpace( v ) );
        }

        public static IValidator<TFact, string> IsNotEmpty<TFact>( this IValidator<TFact, string> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => !(string.IsNullOrWhiteSpace( v )) );
        }

        public static IValidator<TFact, string> HasMinimumLength<TFact>( this IValidator<TFact, string> validator, int minimum ) where TFact : class
        {
            return validator.Is( ( f, v ) => !string.IsNullOrWhiteSpace(v) && v.Length >= minimum );
        }


        public static IValidator<TFact, string> HasMaximumLength<TFact>( this IValidator<TFact, string> validator, int maximum ) where TFact : class
        {
            return validator.Is( ( f, v ) => string.IsNullOrWhiteSpace(v) || v.Length >= maximum );
        }


        public static IValidator<TFact, string> IsIn<TFact>( this IValidator<TFact, string> validator, params string[] values ) where TFact : class
        {
            return validator.Is( ( f, v ) => values.Contains( v ) );
        }


        public static IValidator<TFact, string> IsNotIn<TFact>( this IValidator<TFact, string> validator, params string[] values ) where TFact : class
        {
            return validator.Is( ( f, v ) => !(values.Contains( v )) );
        }


        public static IValidator<TFact, string> IsLessThen<TFact>(  this IValidator<TFact, string> validator, string test ) where TFact : class
        {
            return validator.Is( ( f, v ) => string.Compare( test, v, StringComparison.Ordinal ) == 1 );
        }

        public static IValidator<TFact, string> IsLessThen<TFact>( this IValidator<TFact, string> validator, Func<TFact, string> extractor ) where TFact : class
        {
            return validator.Is((f, v) => string.Compare(extractor(f), v, StringComparison.Ordinal) == 1);
        }


        public static IValidator<TFact, string> IsGreaterThen<TFact>(  this IValidator<TFact, string> validator, string test ) where TFact : class
        {
            return validator.Is( ( f, v ) => string.Compare( test, v, StringComparison.Ordinal ) == -1 );
        }

        public static IValidator<TFact, string> IsGreaterThen<TFact>( this IValidator<TFact, string> validator, Func<TFact, string> extractor ) where TFact : class
        {
            return validator.Is((f, v) => String.Compare(extractor(f), v, StringComparison.Ordinal) == -1);
        }


        public static IValidator<TFact, string> IsEqualTo<TFact>( this IValidator<TFact, string> validator, string test ) where TFact : class
        {
            return validator.Is( ( f, v ) => test.Equals( v ) );
        }

        public static IValidator<TFact, string> IsEqualTo<TFact>( this IValidator<TFact, string> validator, Func<TFact, string> extractor) where TFact : class
        {
            return validator.Is((f, v) => extractor(f).Equals(v) );
        }


        public static IValidator<TFact, string> IsNotEqualTo<TFact>(  this IValidator<TFact, string> validator, string test ) where TFact : class
        {
            return validator.Is( ( f, v ) => !(test.Equals( v )) );
        }

        public static IValidator<TFact, string> IsNotEqualTo<TFact>( this IValidator<TFact, string> validator, Func<TFact,string> extractor ) where TFact : class
        {
            return validator.Is((f, v) => !(extractor(f).Equals(v)));
        }


        public static IValidator<TFact, string> IsMatch<TFact>(  this IValidator<TFact, string> validator, string pattern ) where TFact : class
        {
            return validator.Is( ( f, v ) => Regex.IsMatch( v, pattern ) );
        }

        public static IValidator<TFact, string> IsNotMatch<TFact>(  this IValidator<TFact, string> validator, string pattern ) where TFact : class
        {
            return validator.IsNot( ( f, v ) => Regex.IsMatch( v, pattern ) );
        }

        public static IValidator<TFact, string> IsPhone<TFact>(  this IValidator<TFact, string> validator ) where TFact : class
        {
            return validator.Is((f, v) => string.IsNullOrWhiteSpace(v) || Regex.IsMatch(v, @"\(?(\d{3})\)?-?(\d{3})-(\d{4})"));
        }


        public static IValidator<TFact, string> IsEmail<TFact>( this IValidator<TFact, string> validator) where TFact : class
        {
            return validator.Is((f, v) => string.IsNullOrWhiteSpace(v) || Regex.IsMatch(v, @"^[a-zA-Z]+(([\'\,\.\- ][a-zA-Z ])?[a-zA-Z]*)*\s+&lt;(\w[-._\w]*\w@\w[-._\w]*\w\.\w{2,3})&gt;$|^(\w[-._\w]*\w@\w[-._\w]*\w\.\w{2,3})$"));
        }


        public static IValidator<TFact, string> IsSsn<TFact>( this IValidator<TFact, string> validator ) where TFact : class
        {
            return validator.Is( ( f, v ) => string.IsNullOrWhiteSpace( v ) || Regex.IsMatch( v, @"^\d{3}-\d{2}-\d{4}$" ) );
        }


        public static IValidator<TFact, string> IsZip<TFact>(  this IValidator<TFact, string> validator ) where TFact : class
        {
            return validator.Is((f, v) => string.IsNullOrWhiteSpace(v) || Regex.IsMatch(v, @"^[0-9]{5}(?:-[0-9]{4})?$"));
        }



        // ReSharper disable once InconsistentNaming
        private static readonly ISet<string> _USStates;


        // ReSharper disable once InconsistentNaming
        public static IValidator<TFact, string> IsUSState<TFact>( this IValidator<TFact, string> validator) where TFact : class
        {
            return validator.Is((f, v) => string.IsNullOrWhiteSpace(v) || _USStates.Contains(v));
        }


        // ReSharper disable once InconsistentNaming
        private static readonly ISet<string> _states;


        // ReSharper disable once InconsistentNaming
        public static IValidator<TFact, string> IsState<TFact>( this IValidator<TFact, string> validator) where TFact : class
        {
            return validator.Is((f, v) => string.IsNullOrWhiteSpace(v) || _states.Contains(v));
        }


    }

}
