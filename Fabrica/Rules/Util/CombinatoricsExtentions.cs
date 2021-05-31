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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Fabrica.Rules.Util
{

    [SuppressMessage( "ReSharper", "PossibleMultipleEnumeration" )]
    internal static class CombinatoricsExtentions
    {
        internal static IEnumerable<IEnumerable<T>> Combinations<T>( this IEnumerable<T> elements, int k )
        {
            return k == 0 ? new[] {new T[0]} : elements.SelectMany( ( e, i ) => elements.Skip( i + 1 ).Combinations( k - 1 ).Select( c => (new[] {e}).Concat( c ) ) );
        }

        internal static IEnumerable<IEnumerable<T>> CombinationsWithRepitition<T>( this IEnumerable<T> elements, int k )
        {
            return k == 0 ? new[] {new T[0]} : elements.SelectMany( ( e, i ) => elements.CombinationsWithRepitition( k - 1 ).Select( c => (new[] {e}).Concat( c ) ) );
        }


        [JetBrains.Annotations.NotNull]
        internal static IEnumerable<IEnumerable<T>> Variations<T>( this IEnumerable<T> elements, int k )
        {
            IList<IEnumerable<T>> results = new List<IEnumerable<T>>();

            for( var x = 1; x <= k; x++ )
            {
                foreach( var cmb in elements.Combinations( x ) )
                    results.Add( cmb );
            }

            return results;
        }

        [JetBrains.Annotations.NotNull]
        internal static IEnumerable<IEnumerable<T>> VariationsWithRepitition<T>( this IEnumerable<T> elements, int k )
        {
            IList<IEnumerable<T>> results = new List<IEnumerable<T>>();

            for( var x = 1; x <= k; x++ )
            {
                foreach( var cmb in elements.CombinationsWithRepitition( x ) )
                    results.Add( cmb );
            }

            return results;
        }
    }

}
