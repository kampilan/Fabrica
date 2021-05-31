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
using Fabrica.Exceptions;
using Fabrica.Rules.Builder;
using Fabrica.Rules.Evaluation;
using JetBrains.Annotations;

namespace Fabrica.Rules
{

    public class ForeachRule<TParent,TFact>: AbstractRule
    {

        public ForeachRule( Func<TParent,IEnumerable<TFact>> extractor,  string setName, string ruleName ): base( setName, ruleName )
        {

            Extractor = extractor;

            Conditions = new List<Func<TFact, bool>>();
            Consequence = null;

        }

        private Func<TParent, IEnumerable<TFact>> Extractor { get; }

        public bool Negated { get; private set; }

        protected IList<Func<TFact, bool>> Conditions { get; set; }
        protected Action<TFact> Consequence { get; set; }
        protected Func<TParent, object> ModifyFunc { get; set; }

        [NotNull]
        public ForeachRule<TParent,TFact> WithSalience( int value )
        {
            Salience = value;
            return this;
        }


        [NotNull]
        public ForeachRule<TParent, TFact> WithInception( DateTime inception )
        {
            Inception = inception;
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> WithExpiration( DateTime expiration )
        {
            Expiration = expiration;
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> FireOnce()
        {
            OnlyFiresOnce = true;
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> FireAlways()
        {
            OnlyFiresOnce = false;
            return this;
        }


        [NotNull]
        public ForeachRule<TParent, TFact> If( Func<TFact, bool> oCondition )
        {
            Conditions.Add( oCondition );
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> And( Func<TFact, bool> oCondition )
        {
            Conditions.Add( oCondition );
            return this;
        }


        [NotNull]
        public ForeachRule<TParent, TFact> Then( Action<TFact> oConsequence )
        {
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> Then( string template, params Func<TFact, object>[] parameters )
        {
            Consequence = f => _BuildMessage( f, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> Then( string group, string template, params Func<TFact, object>[] parameters)
        {
            Consequence = f => _BuildMessage(f, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }


        [NotNull]
        public ForeachRule<TParent, TFact> Then( EventDetail.EventCategory category, string group, string template, params Func<TFact, object>[] parameters )
        {
            Consequence = f => _BuildMessage( f, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> ThenAffirm( int weight )
        {
            Consequence = f => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> ThenVeto( int weight )
        {
            Consequence = s => _HandleVeto( weight );
            return this;
        }


        [NotNull]
        public ForeachRule<TParent, TFact> Fire( Action<TFact> oConsequence )
        {
            Conditions.Add( f => true );
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> Fire( string template, params Func<TFact, object>[] parameters )
        {
            Conditions.Add( f => true );
            Consequence = f => _BuildMessage( f, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> Fire( string group, string template, params Func<TFact, object>[] parameters)
        {
            Conditions.Add(f => true);
            Consequence = f => _BuildMessage(f, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> Fire( EventDetail.EventCategory category, string group, string template, params Func<TFact, object>[] parameters )
        {
            Conditions.Add( f => true );
            Consequence = f => _BuildMessage( f, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> FireAffirm( int weight )
        {
            Conditions.Add( f => true );
            Consequence = f => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> FireVeto( int weight )
        {
            Conditions.Add( f => true );
            Consequence = f => _HandleVeto( weight );
            return this;
        }


        private void _HandleAffirm( int weight )
        {
            RuleThreadLocalStorage.CurrentContext.Results.TotalAffirmations += weight;
        }


        private void _HandleVeto( int weight )
        {
            RuleThreadLocalStorage.CurrentContext.Results.TotalVetos += weight;
        }


        [NotNull]
        public ForeachRule<TParent, TFact> Otherwise( Action<TFact> oConsequence )
        {
            Negated = true;
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> Otherwise( string template, params Func<TFact, object>[] parameters )
        {
            Negated = true;
            Consequence = f => _BuildMessage( f, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> Otherwise( string group, string template, params Func<TFact, object>[] parameters)
        {
            Negated = true;
            Consequence = f => _BuildMessage(f, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> Otherwise(EventDetail.EventCategory category, string group, string template, params Func<TFact, object>[] parameters )
        {
            Negated = true;
            Consequence = f => _BuildMessage( f, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> OtherwiseAffirm( int weight )
        {
            Negated = true;
            Consequence = f => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> OtherwiseVeto( int weight )
        {
            Negated = true;
            Consequence = f => _HandleVeto( weight );
            return this;
        }

        [NotNull]
        public ForeachRule<TParent, TFact> Modifies()
        {
            ModifyFunc = f => f;
            return this;
        }


        private void _BuildMessage( TFact fact, EventDetail.EventCategory category, string group, string template, [NotNull] Func<TFact, object>[] parameters )
        {
            int len = parameters.Length;

            var markers = new object[len];
            for (int i = 0; i < len; i++)
            {
                object o = parameters[i]( fact ) ?? "null";
                markers[i] = o;
            }

            string desc = String.Format( template, markers );
            RuleThreadLocalStorage.CurrentContext.Event( category, group, desc, fact );
        }


        protected override IRule InternalEvaluate( [NotNull] object[] offered )
        {

            base.InternalEvaluate( offered );

            var parent = (TParent)offered[0];

            var trueFacts = new List<TFact>();

            foreach( TFact fact in Extractor( parent ) )
            {

                foreach( var cond in Conditions )
                {

                    bool bResult = cond( fact );

                    if( !(bResult) && !(Negated) )
                        break;

                    if( bResult && Negated )
                        break;

                    trueFacts.Add( fact );

                }

            }


            if( trueFacts.Count > 0 )
            {
                var sub = new SubRule<TParent,TFact>( trueFacts, Consequence, parent, ModifyFunc )
                {
                    Namespace = Namespace,
                    Name = Name,
                    Inception = Inception,
                    Expiration = Expiration,
                    Mutex = "",
                    OnlyFiresOnce = OnlyFiresOnce,
                    Salience = Salience
                };

                return sub;
            }

            return null;

        }


        protected override void InternalFire( object[] offered )
        {

        }


    }

}
