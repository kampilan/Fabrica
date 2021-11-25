/*
The MIT License (MIT)

Copyright (c) 2019 The Kampilan Group Inc.

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
using System.Linq.Expressions;
using Fabrica.Rql.Parser;
using JetBrains.Annotations;

namespace Fabrica.Rql.Builder
{


    public class RqlFilterBuilder<TTarget>: AbstractFilterBuilder<RqlFilterBuilder<TTarget>>, IRqlFilter<TTarget> where TTarget: class
    {

        public static RqlFilterBuilder<TTarget> Create()
        {
            return new RqlFilterBuilder<TTarget>();
        }


        public static RqlFilterBuilder<TTarget> Where<TValue>([NotNull] Expression<Func<TTarget, TValue>> prop )
        {
            var builder = new RqlFilterBuilder<TTarget>().And(prop);
            return builder;
        }

        public static RqlFilterBuilder<TTarget> All()
        {
            var builder = new RqlFilterBuilder<TTarget>();
            return builder;
        }



        protected RqlFilterBuilder()
        {
            
        }

        public RqlFilterBuilder([NotNull] RqlTree tree ) : base( tree )
        {
            
        }


        public RqlFilterBuilder<TTarget> AutoProject()
        {

            ProjectedProperties.Clear();
            foreach (var prop in typeof(TTarget).GetProperties() )
            {
                if( ProjectableTypes.IsProjectable(prop.PropertyType) )
                    ProjectedProperties.Add(prop.Name);
            }

            return this;

        }


        [NotNull]
        public RqlFilterBuilder<TTarget> And<TValue>([NotNull] Expression<Func<TTarget, TValue>> prop )
        {

            if (prop == null) throw new ArgumentNullException(nameof(prop));

            if (prop.Body == null)
                throw new ArgumentNullException(nameof(prop));

            if (!(prop.Body is MemberExpression))
                throw new ArgumentException("Targets of a builder must be a field or a property on the output model");

            var propExpr = (MemberExpression)prop.Body;

            CurrentName = propExpr.Member.Name;

            return this;

        }


        [NotNull]
        public override Type Target => typeof(TTarget);



    }


    public class RqlFilterBuilder : AbstractFilterBuilder<RqlFilterBuilder>
    {


        public static RqlFilterBuilder Create()
        {
            return new RqlFilterBuilder();
        }


        public static RqlFilterBuilder All()
        {
            var builder = new RqlFilterBuilder();
            return builder;
        }




        public static RqlFilterBuilder Where( [NotNull] string prop )
        {

            if (string.IsNullOrWhiteSpace(prop))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(prop));

            var builder = new RqlFilterBuilder().And(prop);
            return builder;
        }


        protected RqlFilterBuilder()
        {

        }

        public RqlFilterBuilder( [NotNull] RqlTree tree) : base(tree)
        {

        }


        [NotNull]
        public override Type Target => typeof(RqlFilterBuilder);

        public bool IsAll => !HasCriteria;


        [NotNull]
        public RqlFilterBuilder And( [NotNull] string prop )
        {

            if (string.IsNullOrWhiteSpace( prop ))
                throw new ArgumentNullException(nameof(prop));

            CurrentName = prop;

            return this;

        }


    }


}
