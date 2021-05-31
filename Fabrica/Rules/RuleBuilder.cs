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
using JetBrains.Annotations;

namespace Fabrica.Rules
{


    /// <summary>
    /// Responsible for the creation and collection of rules that reason over a single
    /// fact. The single fact RuleBuilder allows for the creation or both normal rules
    /// and the special case validation rules.
    /// At runtime RuleBuilders are discovered in supplied assemblies and initialied to
    /// create and collect into the a RuleBase. The RuleBase then serves up the rules
    /// for evaluation.
    /// </summary>
    public abstract class RuleBuilder : AbstractRuleBuilder, IBuilder
    {



        /// <summary>
        /// Adds a rule that reasons over the single fact type defined for this builder
        /// </summary>
        /// <param name="ruleName">The name for the rule. This is required and should be
        /// unique within the builder where the rule is defined. I can be anything you like
        /// and serves no operational function. However it is very useful when you are
        /// troubleshoot your rules and is used in the EvaluationResults statistics.</param>
        /// <returns>
        /// The newly created rule for the single given fact type of this builder
        /// </returns>
        [NotNull]
        public virtual Rule<TFact> AddRule<TFact>(string ruleName)
        {

            if (ruleName == null)
                throw new ArgumentNullException(nameof(ruleName));

            if (ruleName == "")
                throw new ArgumentException("All rules must have names and should be unique within a given builder", nameof(ruleName));


            string nameSpace = GetType().Namespace;
            string fullSetName = $"{nameSpace}.{SetName}";

            var rule = new Rule<TFact>(fullSetName, ruleName);

            // Apply the default for FireOnce
            if (DefaultFireOnce)
                rule.FireOnce();
            else
                rule.FireAlways();


            // Apply default salience
            rule.WithSalience(DefaultSalience);

            // Apply default inception and expiration
            rule.WithInception(DefaultInception);
            rule.WithExpiration(DefaultExpiration);


            Sinks.Add( t => t.Add( typeof(TFact), rule ) );

            return rule;

        }


        /// <summary>
        /// Adds a validation rule for the single fact type defined for this builder.
        /// Validation rules are a special case of the regular rule. They typically have
        /// special condition builders asscoaied with them that allow for easy definition of
        /// validation conditions that must evaluate to true for the given data expression
        /// associated with the given fact to be considered "valid". They do not modifiy the
        /// facts that validate but instead only report instances when the conditions for
        /// validation have not been met.
        /// </summary>
        /// <param name="ruleName">The name for the rule. This is required and should be
        /// unique within the builder where the rule is defined. I can be anything you like
        /// and serves no operational function. However it is very useful when you are
        /// troubleshooting your rules and is used in the EvaluationResults statistics.</param>
        /// <returns>
        /// The newly created rule for the single given fact type of this builder
        /// </returns>
        /// <example>
        /// AddValidation("NameIsNotEmpty")
        ///      .Assert( f=&gt;f.Name).IsNotEmpty()
        ///      .Otherwise( "Name is empty" );
        /// AddValidation("NameIsAtLeast5Long")
        ///      .Assert( f=&gt;f.Name).HasMinimumLength(5)
        ///      .Otherwise( "The value specified for Name ({0}) is to short.", f=&gt;f.Name );
        /// </example>
        [NotNull]
        public virtual ValidationRule<TFact> AddValidation<TFact>(string ruleName)
        {

            if (ruleName == null)
                throw new ArgumentNullException(nameof(ruleName));

            if (String.IsNullOrWhiteSpace(ruleName))
                throw new ArgumentException("All rules must have a name and should be unique", nameof(ruleName));


            string nameSpace = GetType().Namespace;
            string fullSetName = $"{nameSpace}.{SetName}";

            var rule = new ValidationRule<TFact>(fullSetName, ruleName);

            // Apply default salience
            rule.WithSalience(DefaultSalience);

            // Apply default inception and expiration
            rule.WithInception(DefaultInception);
            rule.WithExpiration(DefaultExpiration);

            Sinks.Add(t => t.Add(typeof(TFact), rule));


            return rule;

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
        /// var rule = AddRule( "Gabby Foreach rule", p=&gt;p.Chilldren ).Modifies()
        ///     If( c=&gt;c.Name == "Gabby" ).And( c=&gt;c.Age == 4 )
        ///     Then( c=&gt;c.Status = "Not A baby anymore" )
        /// </example>
        [NotNull]
        public virtual ForeachRule<TFact, TChild> AddRule<TFact,TChild>(string ruleName, Func<TFact, IEnumerable<TChild>> extractor)
        {

            if (ruleName == null)
                throw new ArgumentNullException(nameof(ruleName));

            if (String.IsNullOrWhiteSpace(ruleName))
                throw new ArgumentException("All rules must have a name and should be unique", nameof(ruleName));

            string nameSpace = GetType().Namespace;
            string fullSetName = $"{nameSpace}.{SetName}";

            var rule = new ForeachRule<TFact, TChild>(extractor, fullSetName, ruleName);

            // Apply default salience
            rule.WithSalience(DefaultSalience);

            // Apply default inception and expiration
            rule.WithInception(DefaultInception);
            rule.WithExpiration(DefaultExpiration);

            Sinks.Add(t => t.Add(typeof(TFact), rule));

            return rule;

        }


    }



    /// <summary>
    /// Responsible for the creation and collection of rules that reason over a single
    /// fact. The single fact RuleBuilder allows for the creation or both normal rules
    /// and the special case validation rules.
    /// At runtime RuleBuilders are discovered in supplied assemblies and initialied to
    /// create and collect into the a RuleBase. The RuleBase then serves up the rules
    /// for evaluation.
    /// </summary>
    /// <typeparam name="TFact">The Type of fact that this rule reasons over</typeparam>
    public abstract class RuleBuilder<TFact> : AbstractRuleBuilder, IBuilder
    {


        protected RuleBuilder()
        {
            Targets = new[] {typeof(TFact)};
        }


        /// <summary>
        /// Adds a rule that reasons over the single fact type defined for this builder
        /// </summary>
        /// <param name="ruleName">The name for the rule. This is required and should be
        /// unique within the builder where the rule is defined. I can be anything you like
        /// and serves no operational function. However it is very useful when you are
        /// troubleshoot your rules and is used in the EvaluationResults statistics.</param>
        /// <returns>
        /// The newly created rule for the single given fact type of this builder
        /// </returns>
        [NotNull]
        public virtual Rule<TFact> AddRule( string ruleName )
        {
            if (ruleName == null)
                throw new ArgumentNullException( nameof(ruleName) );

            if (ruleName == "")
                throw new ArgumentException( "All rules must have names and should be unique within a given builder", nameof(ruleName) );


            string nameSpace = GetType().Namespace;
            string fullSetName = $"{nameSpace}.{SetName}";

            var rule = new Rule<TFact>( fullSetName, ruleName );

            // Apply the default for FireOnce
            if (DefaultFireOnce)
                rule.FireOnce();
            else
                rule.FireAlways();


            // Apply default salience
            rule.WithSalience( DefaultSalience );

            // Apply default inception and expiration
            rule.WithInception( DefaultInception );
            rule.WithExpiration( DefaultExpiration );

            Rules.Add( rule );

            return rule;
        }


        /// <summary>
        /// Adds a validation rule for the single fact type defined for this builder.
        /// Validation rules are a special case of the regular rule. They typically have
        /// special condition builders asscoaied with them that allow for easy definition of
        /// validation conditions that must evaluate to true for the given data expression
        /// associated with the given fact to be considered "valid". They do not modifiy the
        /// facts that validate but instead only report instances when the conditions for
        /// validation have not been met.
        /// </summary>
        /// <param name="ruleName">The name for the rule. This is required and should be
        /// unique within the builder where the rule is defined. I can be anything you like
        /// and serves no operational function. However it is very useful when you are
        /// troubleshooting your rules and is used in the EvaluationResults statistics.</param>
        /// <returns>
        /// The newly created rule for the single given fact type of this builder
        /// </returns>
        /// <example>
        /// AddValidation("NameIsNotEmpty")
        ///      .Assert( f=&gt;f.Name).IsNotEmpty()
        ///      .Otherwise( "Name is empty" );
        /// AddValidation("NameIsAtLeast5Long")
        ///      .Assert( f=&gt;f.Name).HasMinimumLength(5)
        ///      .Otherwise( "The value specified for Name ({0}) is to short.", f=&gt;f.Name );
        /// </example>
        [NotNull]
        public virtual ValidationRule<TFact> AddValidation( string ruleName )
        {
            if (ruleName == null)
                throw new ArgumentNullException( nameof(ruleName) );

            if (String.IsNullOrWhiteSpace( ruleName ))
                throw new ArgumentException( "All rules must have a name and should be unique", nameof(ruleName) );


            string nameSpace = GetType().Namespace;
            string fullSetName = $"{nameSpace}.{SetName}";

            var rule = new ValidationRule<TFact>( fullSetName, ruleName );

            // Apply default salience
            rule.WithSalience( DefaultSalience );

            // Apply default inception and expiration
            rule.WithInception( DefaultInception );
            rule.WithExpiration( DefaultExpiration );

            Rules.Add( rule );

            return rule;
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
        /// var rule = AddRule( "Gabby Foreach rule", p=&gt;p.Chilldren ).Modifies()
        ///     If( c=&gt;c.Name == "Gabby" ).And( c=&gt;c.Age == 4 )
        ///     Then( c=&gt;c.Status = "Not A baby anymore" )
        /// </example>
        [NotNull]
        public virtual ForeachRule<TFact, TChild> AddRule<TChild>( string ruleName, Func<TFact, IEnumerable<TChild>> extractor )
        {

            if (ruleName == null)
                throw new ArgumentNullException( nameof(ruleName) );

            if (String.IsNullOrWhiteSpace( ruleName ))
                throw new ArgumentException( "All rules must have a name and should be unique", nameof(ruleName) );

            string nameSpace = GetType().Namespace;
            string fullSetName = $"{nameSpace}.{SetName}";

            var rule = new ForeachRule<TFact, TChild>( extractor, fullSetName, ruleName );

            // Apply default salience
            rule.WithSalience( DefaultSalience );

            // Apply default inception and expiration
            rule.WithInception( DefaultInception );
            rule.WithExpiration( DefaultExpiration );

            Rules.Add( rule );

            return rule;

        }


    }



    /// <summary>
    /// Responsible for the creation and collection of rules that reason over a two
    /// facts. The two fact RuleBuilder allows for the creation and collection of rules.
    /// At runtime RuleBuilders are discovered in supplied assemblies and initialied to
    /// create and collect into the a RuleBase. The RuleBase then serves up the rules
    /// for evaluation.
    /// </summary>
    /// <typeparam name="TFact1">The first Type of fact that this rule reasons
    /// over</typeparam>
    /// <typeparam name="TFact2">The second Type of fact that this rule reasons
    /// over</typeparam>
    public abstract class RuleBuilder<TFact1, TFact2> : AbstractRuleBuilder, IBuilder
    {


        protected RuleBuilder()
        {
            Targets = new[] { typeof(TFact1), typeof(TFact2) };
        }

        /// <summary>
        /// Adds a rule that reasons over the two fact types defined for this builder
        /// </summary>
        /// <param name="ruleName">The name for the rule. This is required and should be
        /// unique within the builder where the rule is defined. I can be anything you like
        /// and serves no operational function. However it is very useful when you are
        /// troubleshoot your rules and is used in the EvaluationResults statistics.</param>
        /// <returns>
        /// The newly created rule for the two given fact types of this builder
        /// </returns>
        [NotNull]
        public virtual Rule<TFact1, TFact2> AddRule( string ruleName )
        {
            if (ruleName == null)
                throw new ArgumentNullException( nameof(ruleName) );

            if (ruleName == "")
                throw new ArgumentException( "All rules must have names and should be unique within a given builder", nameof(ruleName) );


            string nameSpace = GetType().Namespace;
            string fqSetName = $"{nameSpace}.{SetName}";

            var rule = new Rule<TFact1, TFact2>( fqSetName, ruleName );

            // Apply the default for FireOnce
            if (DefaultFireOnce)
                rule.FireOnce();
            else
                rule.FireAlways();

            // Apply default salience
            rule.WithSalience( DefaultSalience );

            // Apply default inception and expiration
            rule.WithInception( DefaultInception );
            rule.WithExpiration( DefaultExpiration );

            Rules.Add( rule );

            return rule;
        }

    }


    /// <summary>
    /// Responsible for the creation and collection of rules that reason over three
    /// facts. The three fact RuleBuilder allows for the creation and collection of rules.
    /// At runtime RuleBuilders are discovered in supplied assemblies and initialied to
    /// create and collect into the a RuleBase. The RuleBase then serves up the rules
    /// for evaluation.
    /// </summary>
    /// <typeparam name="TFact1">The first Type of fact that this rule reasons
    /// over</typeparam>
    /// <typeparam name="TFact2">The second Type of fact that this rule reasons
    /// over</typeparam>
    /// <typeparam name="TFact3">The third Type of fact that this rule reasons
    /// over</typeparam>
    public abstract class RuleBuilder<TFact1, TFact2, TFact3> : AbstractRuleBuilder, IBuilder
    {

        protected RuleBuilder()
        {
            Targets = new[] { typeof(TFact1), typeof(TFact2), typeof(TFact3) };
        }
        

        /// <summary>
        /// Adds a rule that reasons over the three fact types defined for this builder
        /// </summary>
        /// <param name="ruleName">The name for the rule. This is required and should be
        /// unique within the builder where the rule is defined. I can be anything you like
        /// and serves no operational function. However it is very useful when you are
        /// troubleshoot your rules and is used in the EvaluationResults statistics.</param>
        /// <returns>
        /// The newly created rule for the three given fact types of this builder
        /// </returns>
        [NotNull]
        public virtual Rule<TFact1, TFact2, TFact3> AddRule( string ruleName )
        {
            if( ruleName == null )
                throw new ArgumentNullException( nameof(ruleName) );

            if( ruleName == "" )
                throw new ArgumentException( "All rules must have names and should be unique within a given builder", nameof(ruleName) );


            string nameSpace = GetType().Namespace;
            string fqSetName = $"{nameSpace}.{SetName}";

            var rule = new Rule<TFact1, TFact2, TFact3>( fqSetName, ruleName );

            // Apply the default for FireOnce
            if( DefaultFireOnce )
                rule.FireOnce();
            else
                rule.FireAlways();


            // Apply default salience
            rule.WithSalience( DefaultSalience );

            // Apply default inception and expiration
            rule.WithInception( DefaultInception );
            rule.WithExpiration( DefaultExpiration );

            Rules.Add( rule );

            return rule;
        }
    }


    /// <summary>
    /// Responsible for the creation and collection of rules that reason over four
    /// facts. The four fact RuleBuilder allows for the creation and collection of rules.
    /// At runtime RuleBuilders are discovered in supplied assemblies and initialied to
    /// create and collect into the a RuleBase. The RuleBase then serves up the rules
    /// for evaluation.
    /// </summary>
    /// <typeparam name="TFact1">The first Type of fact that this rule reasons
    /// over</typeparam>
    /// <typeparam name="TFact2">The second Type of fact that this rule reasons
    /// over</typeparam>
    /// <typeparam name="TFact3">The third Type of fact that this rule reasons
    /// over</typeparam>
    /// <typeparam name="TFact4">The fourth Type of fact that this rule reasons
    /// over</typeparam>
    public abstract class RuleBuilder<TFact1, TFact2, TFact3, TFact4> : AbstractRuleBuilder, IBuilder
    {


        protected RuleBuilder()
        {
            Targets = new[] { typeof(TFact1), typeof(TFact2), typeof(TFact3), typeof(TFact4) };
        }



        /// <summary>
        /// Adds a rule that reasons over the four fact types defined for this builder
        /// </summary>
        /// <param name="ruleName">The name for the rule. This is required and should be
        /// unique within the builder where the rule is defined. I can be anything you like
        /// and serves no operational function. However it is very useful when you are
        /// troubleshoot your rules and is used in the EvaluationResults statistics.</param>
        /// <returns>
        /// The newly created rule for the four given fact types of this builder
        /// </returns>
        [NotNull]
        public virtual Rule<TFact1, TFact2, TFact3, TFact4> AddRule( string ruleName )
        {
            if( ruleName == null )
                throw new ArgumentNullException( nameof(ruleName) );

            if( ruleName == "" )
                throw new ArgumentException( "All rules must have names and should be unique within a given builder", nameof(ruleName) );


            string nameSpace = GetType().Namespace;
            string fqSetName = $"{nameSpace}.{SetName}";

            var rule = new Rule<TFact1, TFact2, TFact3, TFact4>( fqSetName, ruleName );

            // Apply the default for FireOnce
            if( DefaultFireOnce )
                rule.FireOnce();
            else
                rule.FireAlways();


            // Apply default salience
            rule.WithSalience( DefaultSalience );

            // Apply default inception and expiration
            rule.WithInception( DefaultInception );
            rule.WithExpiration( DefaultExpiration );

            Rules.Add( rule );

            return rule;
        }
    
    }


}
