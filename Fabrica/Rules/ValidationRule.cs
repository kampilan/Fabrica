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
using System.Linq.Expressions;
using Fabrica.Rules.Builder;
using Fabrica.Rules.Evaluation;
using Fabrica.Rules.Validators;
using JetBrains.Annotations;

namespace Fabrica.Rules
{

    // ReSharper disable once UnusedTypeParameter
    public interface IValidationRule<out TFact>: IRule
    {
        
    }


    public class ValidationRule<TFact> : AbstractRule, IValidationRule<TFact>
    {

        public ValidationRule( string setName, string ruleName ) : base( setName, ruleName )
        {
            Salience = int.MaxValue - 1000000;

            OnlyFiresOnce = true;

            Predicates = new List<Func<TFact, bool>>();
        }


        protected IList<Func<TFact, bool>> Predicates { get; private set; }
        protected BaseValidator<TFact> TypeValidator { get; private set; }

        private Action<TFact> CascadeAction { get; set; }


        [NotNull]
        public ValidationRule<TFact> WithSalience( int value )
        {
            Salience = (int.MaxValue - 1000000) + value;
            return this;
        }

        [NotNull]
        public ValidationRule<TFact> InMutex( string name )
        {
            if( string.IsNullOrWhiteSpace( name ) )
                throw new ArgumentNullException( nameof( name ) );

            Mutex = name;
            return this;
        }


        [NotNull]
        public ValidationRule<TFact> WithInception( DateTime inception )
        {
            Inception = inception;
            return this;
        }


        [NotNull]
        public ValidationRule<TFact> WithExpiration( DateTime expiration )
        {
            Expiration = expiration;
            return this;
        }

        [NotNull]
        public ValidationRule<TFact> When( Func<TFact, bool> predicate )
        {
            if( predicate == null )
                throw new ArgumentNullException( nameof( predicate ) );

            Predicates = new List<Func<TFact, bool>> {predicate};
            return this;
        }


        [NotNull]
        public ValidationRule<TFact> And( Func<TFact, bool> predicate )
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            Predicates.Add(predicate);
            return this;
        }



        [NotNull]
        public IValidator<TFact, TType> Assert<TType>( Expression<Func<TFact, TType>> extractorEx )
        {

            if( extractorEx == null )
                throw new ArgumentNullException( nameof( extractorEx ) );

            var factName = typeof(TFact).Name;

            var group = (extractorEx.Body is MemberExpression body ? $"{typeof(TFact).Name}.{body.Member.Name}" : factName);

            var extractor = extractorEx.Compile();

            var validator = new Validator<TFact, TType>( this, group, extractor );
            TypeValidator = validator;

            return validator;

        }


        [NotNull]
        public EnumerableValidator<TFact, TType> AssertOver<TType>( Expression<Func<TFact, IEnumerable<TType>>> extractorEx )
        {

            if( extractorEx == null )
                throw new ArgumentNullException( nameof( extractorEx ) );

            var factName = typeof(TFact).Name;

            var group = (extractorEx.Body is MemberExpression body ? $"{typeof(TFact).Name}.{body.Member.Name}" : factName);

            var extractor = extractorEx.Compile();

            var validator = new EnumerableValidator<TFact, TType>( this, group, extractor );
            TypeValidator = validator;

            return validator;

        }


        public void Cascade<TRef>( [NotNull] Func<TFact, TRef> extractor ) where TRef : class
        {
            if( extractor == null )
                throw new ArgumentNullException( nameof( extractor ) );

            CascadeAction = f => RuleThreadLocalStorage.CurrentContext.InsertFact( extractor( f ) );
        }


        public void CascadeAll<TChild>( [NotNull] Func<TFact, IEnumerable<TChild>> extractor ) where TChild : class
        {
            if( extractor == null )
                throw new ArgumentNullException( nameof( extractor ) );

            CascadeAction = f => _CascadeCollection( extractor( f ) );
        }

        private void _CascadeCollection( [NotNull] IEnumerable<object> children )
        {
            foreach( var o in children )
                RuleThreadLocalStorage.CurrentContext.InsertFact( o );
        }


        protected override IRule InternalEvaluate( [NotNull] object[] offered  )
        {
            base.InternalEvaluate( offered );

            var fact = (TFact)offered[0];

            if( CascadeAction != null )
                return this;

            if( (Predicates.Count > 0) && (Predicates.Select( cond=>cond(fact) ).Any( r=>r == false )) )
                return null;

            if( TypeValidator.Conditions.Select( cond => cond( fact ) ).Any( result => !(result) ) )
                return this;

            return null;

        }


        protected override void InternalFire( [NotNull] object[] offered )
        {
            base.InternalFire( offered );

            var fact = (TFact)offered[0];

            if( CascadeAction != null )
                CascadeAction( fact );
            else
                TypeValidator.Consequence( fact );
        }
    }

}
