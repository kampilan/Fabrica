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
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo( "Nano.Rules.UnitTests" )]

namespace Fabrica.Rules.Evaluation
{

    public class FactSpace
    {

        private IList<object> Facts { get; } = new List<object>();
        private ISet<object> Guard { get; } = new HashSet<object>();

        private IDictionary<Type, Schema> SchemaMap { get; } = new Dictionary<Type, Schema>();
        internal IList<Schema> Schema { get; } = new List<Schema>();

        private int NextPosition { get; set; } = 1;
        private IDictionary<int, int> SelectorMap { get; } = new Dictionary<int, int>();


        internal int TypeCount => Schema.Count;

        [NotNull]
        internal int[] GetIdentityFromSelector( int[] selectorIndices )
        {
            int len = selectorIndices.Length;

            var identityIndices = new int[len];
            for( int i = 0; i < len; i++ )
                identityIndices[i] = SelectorMap[selectorIndices[i]];

            return identityIndices;
        }


        private void _InternalAdd( [NotNull] object fact, int identity )
        {
            if( SelectorMap.Count >= 65535 )
                throw new InvalidOperationException( "Evaluation is limited to 65535 facts at a time." );

            Type factType = fact.GetType();

            if( !(SchemaMap.TryGetValue( factType, out var schema )) )
            {
                schema = new Schema {FactType = factType};

                SchemaMap[factType] = schema;
                Schema.Add( schema );
            }

            // Add the new facts identity to the reference map
            SelectorMap[NextPosition] = identity;

            // Add this reference to the schema for this type
            schema.Members.Add( NextPosition );

            NextPosition++;
        }


        [NotNull]
        internal Type[] GetFactTypes( byte[] signatureIndices )
        {
            int len = signatureIndices.Length;

            var types = new Type[len];

            for( int i = 0; i < len; i++ )
                types[i] = Schema[signatureIndices[i]].FactType;

            return types;
        }


        [NotNull]
        internal object[] GetTuple( int[] selectorIndices )
        {
            int count = selectorIndices.Length;

            var tuple = new object[count];
            for( int i = 0; i < count; i++ )
            {
                int selectorIndex = selectorIndices[i];

                if( SelectorMap.TryGetValue( selectorIndex, out var identityIndex ) )
                    tuple[i] = Facts[identityIndex];
                else
                    return new object[] {};
            }

            return tuple;
        }


        internal void InsertFact( object fact )
        {
            Add( fact );
        }

        internal void ModifyFact( int selectorIndex )
        {
            var identityIndex = SelectorMap[selectorIndex];
            SelectorMap.Remove( selectorIndex );

            var fact = Facts[identityIndex];

            _InternalAdd( fact, identityIndex );

        }

        internal void RetractFact( int reference )
        {
            SelectorMap.Remove( reference );
        }


        public void Add( params object[] facts )
        {
            AddAll( facts );
        }

        public void AddAll( IEnumerable<object> facts )
        {

            foreach (var f in facts.Where(f => !Guard.Contains(f)))
            {

                Facts.Add(f);
                Guard.Add(f);

                _InternalAdd(f, (Facts.Count - 1));
            }

        }

    }

}
