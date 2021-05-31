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
using Fabrica.Rules.Builder;
using Fabrica.Rules.Tree;
using JetBrains.Annotations;

namespace Fabrica.Rules.Factory
{

    public class RuleSet : AbstractRuleSet
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly ISet<string> _namespaces = new HashSet<string>();
        private readonly RuleTree _tree = new RuleTree();

        public IRuleBase RuleBase => _tree;

        public void Add( [NotNull] IBuilder builder )
        {
            builder.LoadRules( _tree );
        }


        protected override IRuleBase GetRuleBase()
        {
            return _tree;
        }

        protected override IEnumerable<string> GetNamespaces()
        {
            return _namespaces;
        }


        [NotNull]
        public Rule<TFact> AddRule<TFact>( string name ) where TFact : class
        {
            var rule = new Rule<TFact>( "runtime", name );
            _tree.Add( new[] {typeof(TFact)}, new IRule[] {rule} );
            return rule;
        }


        [NotNull]
        public override EvaluationContext GetEvaluationContext()
        {
            return new EvaluationContext();
        }
        

        /// <summary>
        /// Adds a rule that reasons over the an enumeration of child facts associated with
        /// the type defined for this builder.
        /// </summary>
        /// <remarks>
        /// These child facts are not inserted into the fact space and thus do not trigger
        /// forward chaining if they are modified. However the parent can signal
        /// modification and trigger forward chaining. If it is required that the children
        /// participate in forward chainging use Cascade instead.
        /// </remarks>
        /// <typeparam name="TFact">The parent fact that contains the children that will
        /// actually be evaluated. Also scheduling and order are alos drive by this type as
        /// opposed to the children</typeparam>
        /// <typeparam name="TChild">The type the conditions and consequence are targeting.
        /// Each rule that produces a true evaluation will have its consequence
        /// fired.</typeparam>
        /// <param name="ruleName">The name for the rule. This is required and should be
        /// unique within the builder where the rule is defined. I can be anything you like
        /// and serves no operational function. However it is very useful when you are
        /// troubleshoot (logging) your rules and is used in the EvaluationResults
        /// statistics.</param>
        /// <param name="extractor">The extractor used to access the collection from the
        /// parent fact. The rules are defined for these child facts. The conditions are
        /// evaulated for each child and those that produce a true condition are
        /// fired.</param>
        /// <returns>
        /// The newly created rule for the single given fact type of this builder
        /// </returns>
        /// <example>
        /// var rule = AddRule&lt;Family&gt;( "Gabby Foreach rule", p=&gt;p.Chilldren ).Modifies()
        ///     If( c=&gt;c.Name == "Gabby" ).And( c=&gt;c.Age == 4 )
        ///     Then( c=&gt;c.Status = "Not A baby anymore" )
        /// </example>
        [NotNull]
        public virtual ForeachRule<TFact, TChild> AddRule<TFact, TChild>( string ruleName, Func<TFact, IEnumerable<TChild>> extractor ) where TFact : class where TChild : class
        {

            if (ruleName == null)
                throw new ArgumentNullException( nameof(ruleName) );

            if (String.IsNullOrWhiteSpace( ruleName ))
                throw new ArgumentException( "All rules must have a name and should be unique", nameof(ruleName) );

            if (extractor == null)
                throw new ArgumentNullException( nameof(extractor) );

            var rule = new ForeachRule<TFact, TChild>( extractor, "runtime", ruleName );

            _tree.Add( new[] { typeof( TFact ) }, new IRule[] { rule } );

            return rule;

        }



        [NotNull]
        public ValidationRule<TFact> AddValidation<TFact>( string name ) where TFact : class
        {
            if( name == null )
                throw new ArgumentNullException( nameof(name) );

            if( name == "" )
                throw new ArgumentException( "All rules must have a name and should be unique", nameof(name) );

            var rule = new ValidationRule<TFact>( "runtime", name );
            _tree.Add( new[] {typeof(TFact)}, new IRule[] {rule} );

            return rule;
        }


        [NotNull]
        public Rule<TFact1, TFact2> AddRule<TFact1, TFact2>( string name )
            where TFact1 : class
            where TFact2 : class
        {
            var rule = new Rule<TFact1, TFact2>( "runtime", name );
            _tree.Add( new[] {typeof(TFact1), typeof(TFact2)}, new IRule[] {rule} );
            return rule;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> AddRule<TFact1, TFact2, TFact3>( string name )
            where TFact1 : class
            where TFact2 : class
            where TFact3 : class
        {
            var rule = new Rule<TFact1, TFact2, TFact3>( "runtime", name );
            _tree.Add( new[] {typeof(TFact1), typeof(TFact2), typeof(TFact3)}, new IRule[] {rule} );
            return rule;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> AddRule<TFact1, TFact2, TFact3, TFact4>( string name )
            where TFact1 : class
            where TFact2 : class
            where TFact3 : class
            where TFact4 : class
        {
            var rule = new Rule<TFact1, TFact2, TFact3,TFact4>( "runtime", name );
            _tree.Add( new[] { typeof( TFact1 ), typeof( TFact2 ), typeof( TFact3 ), typeof(TFact4) }, new IRule[] { rule } );
            return rule;
        }


        public void Clear()
        {
            _tree.Clear();
        }
    }

}
