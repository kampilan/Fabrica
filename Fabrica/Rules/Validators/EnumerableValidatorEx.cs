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

    public static class EnumerableValidatorEx
    {

        public static IEnumerableValidator<TFact, TType> Required<TFact, TType>([NotNull] this IEnumerableValidator<TFact, TType> validator) where TFact : class where TType : class
        {
            return validator.Is((f, v) => v.Any());
        }

        public static IEnumerableValidator<TFact, TType> IsEmpty<TFact, TType>( [NotNull] this IEnumerableValidator<TFact, TType> validator ) where TFact : class where TType : class
        {
            return validator.IsNot( ( f, v ) => v.Any() );
        }

        public static IEnumerableValidator<TFact, TType> IsNotEmpty<TFact, TType>( [NotNull] this IEnumerableValidator<TFact, TType> validator ) where TFact : class where TType : class
        {
            return validator.Is( ( f, v ) => v.Any() );
        }


        [NotNull]
        public static IEnumerableValidator<TFact, TType> Has<TFact, TType>( this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate ) where TFact : class
            where TType : class
        {
            validator.Is( ( f, v ) => v.Any( predicate ) );
            return validator;
        }


        [NotNull]
        public static IEnumerableValidator<TFact, TType> HasNone<TFact, TType>( this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate ) where TFact : class
            where TType : class
        {
            validator.IsNot( ( f, v ) => v.Any( predicate ) );
            return validator;
        }


        [NotNull]
        public static IEnumerableValidator<TFact, TType> HasExactly<TFact, TType>( this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate, int count )
            where TFact : class where TType : class
        {
            validator.Is( ( f, v ) => v.Where( predicate ).Count() == count );
            return validator;
        }


        [NotNull]
        public static IEnumerableValidator<TFact, TType> HasOnlyOne<TFact, TType>( this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate )
            where TFact : class where TType : class
        {
            validator.Is( ( f, v ) => v.Where( predicate ).Count() == 1 );
            return validator;
        }


        [NotNull]
        public static IEnumerableValidator<TFact, TType> HasAtMostOne<TFact, TType>( this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate )
            where TFact : class where TType : class
        {
            validator.Is( ( f, v ) => v.Where( predicate ).Count() <= 1 );
            return validator;
        }


        [NotNull]
        public static IEnumerableValidator<TFact, TType> HasAtLeast<TFact, TType>( this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate, int count )
            where TFact : class where TType : class
        {
            validator.Is( ( f, v ) => v.Where( predicate ).Count() >= count );
            return validator;
        }


        [NotNull]
        public static IEnumerableValidator<TFact, TType> HasAtMost<TFact, TType>( this IEnumerableValidator<TFact, TType> validator, Func<TType, bool> predicate, int count )
            where TFact : class where TType : class
        {
            validator.Is( ( f, v ) => v.Where( predicate ).Count() <= count );
            return validator;
        }

    }



}
