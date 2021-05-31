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
using Fabrica.Exceptions;
using Fabrica.Rules.Builder;
using Fabrica.Rules.Evaluation;
using JetBrains.Annotations;

namespace Fabrica.Rules
{

    public class Rule<TFact> : AbstractRule
    {
        public Rule( string setName, string ruleName ) : base( setName, ruleName )
        {
            Negated = false;

            Conditions = new List<Func<TFact, bool>>();
            Consequence = null;
        }

        public bool Negated { get; private set; }

        private Action<TFact> CascadeAction { get; set; }

        protected IList<Func<TFact, bool>> Conditions { get; set; }
        protected Action<TFact> Consequence { get; set; }
        protected Func<TFact, object> ModifyFunc { get; set; }

        [NotNull]
        public Rule<TFact> WithSalience( int value )
        {
            Salience = value;
            return this;
        }

        [NotNull]
        public Rule<TFact> InMutex( string name )
        {
            Mutex = name;
            return this;
        }


        [NotNull]
        public Rule<TFact> WithInception( DateTime inception )
        {
            Inception = inception;
            return this;
        }

        [NotNull]
        public Rule<TFact> WithExpiration( DateTime expiration )
        {
            Expiration = expiration;
            return this;
        }

        [NotNull]
        public Rule<TFact> FireOnce()
        {
            OnlyFiresOnce = true;
            return this;
        }

        [NotNull]
        public Rule<TFact> FireAlways()
        {
            OnlyFiresOnce = false;
            return this;
        }


        [NotNull]
        public Rule<TFact> If( Func<TFact, bool> oCondition )
        {
            Conditions.Add( oCondition );
            return this;
        }

        [NotNull]
        public Rule<TFact> And( Func<TFact, bool> oCondition )
        {
            Conditions.Add( oCondition );
            return this;
        }


        [NotNull]
        public Rule<TFact> NoConsequence()
        {
            Consequence = f => { };
            return this;
        }


        [NotNull]
        public Rule<TFact> Then( Action<TFact> oConsequence )
        {
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public Rule<TFact> Then( string template, params Func<TFact, object>[] parameters )
        {
            Consequence = f => _BuildMessage( f, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact> Then( string group, string template, params Func<TFact, object>[] parameters)
        {
            Consequence = f => _BuildMessage(f, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }


        [NotNull]
        public Rule<TFact> Then(EventDetail.EventCategory category, string group, string template, params Func<TFact, object>[] parameters )
        {
            Consequence = f => _BuildMessage( f, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact> ThenAffirm( int weight )
        {
            Consequence = f => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact> ThenVeto( int weight )
        {
            Consequence = s => _HandleVeto( weight );
            return this;
        }


        [NotNull]
        public Rule<TFact> Fire( Action<TFact> oConsequence )
        {
            Conditions.Add( f => true );
            Consequence = oConsequence;
            return this;
        }


        [NotNull]
        public Rule<TFact> Fire( string template, params Func<TFact, object>[] parameters )
        {
            Conditions.Add( f => true );
            Consequence = f => _BuildMessage( f, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact> Fire( string group, string template, params Func<TFact, object>[] parameters)
        {
            Conditions.Add(f => true);
            Consequence = f => _BuildMessage(f, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }


        [NotNull]
        public Rule<TFact> Fire(EventDetail.EventCategory category, string group, string template, params Func<TFact, object>[] parameters )
        {
            Conditions.Add( f => true );
            Consequence = f => _BuildMessage( f, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact> FireAffirm( int weight )
        {
            Conditions.Add( f => true );
            Consequence = f => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact> FireVeto( int weight )
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
        public Rule<TFact> Otherwise( Action<TFact> oConsequence )
        {
            Negated = true;
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public Rule<TFact> Otherwise( string template, params Func<TFact, object>[] parameters )
        {
            Negated = true;
            Consequence = f => _BuildMessage( f, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact> Otherwise( string group, string template, params Func<TFact, object>[] parameters)
        {
            Negated = true;
            Consequence = f => _BuildMessage(f, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }


        [NotNull]
        public Rule<TFact> Otherwise(EventDetail.EventCategory category, string group, string template, params Func<TFact, object>[] parameters )
        {
            Negated = true;
            Consequence = f => _BuildMessage( f, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact> OtherwiseAffirm( int weight )
        {
            Negated = true;
            Consequence = f => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact> OtherwiseVeto( int weight )
        {
            Negated = true;
            Consequence = f => _HandleVeto( weight );
            return this;
        }


        private void _BuildMessage( TFact fact, EventDetail.EventCategory category, string group, string template, [NotNull] Func<TFact, object>[] parameters )
        {
            var len = parameters.Length;

            var markers = new object[len];
            for( int i = 0; i < len; i++ )
            {
                var o = parameters[i]( fact ) ?? "null";
                markers[i] = o;
            }

            var desc = String.Format( template, markers );
            RuleThreadLocalStorage.CurrentContext.Event( category, group, desc, fact );
        }


        [NotNull]
        public Rule<TFact> Modifies( Func<TFact, object> modifyFunc )
        {
            ModifyFunc = modifyFunc;
            return this;
        }


        public void Cascade<TRef>( [NotNull] Func<TFact, TRef> extractor ) where TRef : class
        {

            if (extractor == null)
                throw new ArgumentNullException( nameof(extractor) );

            CascadeAction = f => RuleThreadLocalStorage.CurrentContext.InsertFact( extractor( f ) );

        }


        public void CascadeAll<TChild>( [NotNull] Func<TFact, IEnumerable<TChild>> extractor ) where TChild : class
        {
            if (extractor == null)
                throw new ArgumentNullException( nameof(extractor) );

            CascadeAction = f => _CascadeCollection( extractor( f ) );
        }

        private void _CascadeCollection( [NotNull] IEnumerable<object> children )
        {
            foreach (object o in children)
                RuleThreadLocalStorage.CurrentContext.InsertFact( o );
        }


        protected override IRule InternalEvaluate( object[] offered  )
        {

            if( CascadeAction != null )
                return this;

            
            base.InternalEvaluate( offered );

            var fact = (TFact)offered[0];



            // ***********************************************************************
            foreach( var result in Conditions.Select( cond => cond( fact ) ) )
            {

                if( !(result) && !(Negated) )
                    return null;

                if( result && Negated )
                    return null;

            }

            return this;

        }


        protected override void InternalFire( [NotNull] object[] offered )
        {

            var fact = (TFact)offered[0];

            if( CascadeAction != null )
            {
                CascadeAction(fact);
                return;
            }
            
            base.InternalFire( offered );

            Consequence( fact );

            if( ModifyFunc != null )
            {
                object modified = ModifyFunc( fact );
                if( modified is Array )
                {
                    foreach( object o in (modified as Array) )
                        RuleThreadLocalStorage.CurrentContext.ModifyFact( o );
                }
                else if( (modified != null) )
                    RuleThreadLocalStorage.CurrentContext.ModifyFact( modified );
            }

        }

    }


    public class Rule<TFact1, TFact2> : AbstractRule
    {

        public Rule( string setName, string ruleName ): base( setName, ruleName )
        {
            Negated = false;

            Conditions = new List<Func<TFact1, TFact2, bool>>();
            Consequence = null;
        }

        public bool Negated { get; private set; }


        protected IList<Func<TFact1, TFact2, bool>> Conditions { get; set; }
        protected Action<TFact1, TFact2> Consequence { get; set; }
        protected Func<TFact1, TFact2, object> ModifyFunc { get; set; }

        [NotNull]
        public Rule<TFact1, TFact2> WithSalience( int value )
        {
            Salience = value;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> InMutex( string name )
        {
            Mutex = name;
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2> WithInception( DateTime inception )
        {
            Inception = inception;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> WithExpiration( DateTime expiration )
        {
            Expiration = expiration;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> FireOnce()
        {
            OnlyFiresOnce = true;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> FireAlways()
        {
            OnlyFiresOnce = false;
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2> If( Func<TFact1, TFact2, bool> oCondition )
        {
            Conditions.Add( oCondition );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> And( Func<TFact1, TFact2, bool> oCondition )
        {
            Conditions.Add( oCondition );
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2> Then( Action<TFact1, TFact2> oConsequence )
        {
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> Then( string template, params Func<TFact1, TFact2, object>[] parameters )
        {
            Consequence = ( f1, f2 ) => _BuildMessage( f1, f2, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> Then( string group, string template, params Func<TFact1, TFact2, object>[] parameters)
        {
            Consequence = (f1, f2) => _BuildMessage(f1, f2, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2> Then(EventDetail.EventCategory category, string group, string template, params Func<TFact1, TFact2, object>[] parameters )
        {
            Consequence = ( f1, f2 ) => _BuildMessage( f1, f2, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> ThenAffirm( int weight )
        {
            Consequence = ( f1, f2 ) => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> ThenVeto( int weight )
        {
            Consequence = ( f1, f2 ) => _HandleVeto( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> Fire( Action<TFact1, TFact2> oConsequence )
        {
            Conditions.Add( ( f1, f2 ) => true );
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> Fire( string template, params Func<TFact1, TFact2, object>[] parameters )
        {
            Conditions.Add( ( f1, f2 ) => true );
            Consequence = ( f1, f2 ) => _BuildMessage( f1, f2, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> Fire( string group, string template, params Func<TFact1, TFact2, object>[] parameters)
        {
            Conditions.Add((f1, f2) => true);
            Consequence = (f1, f2) => _BuildMessage(f1, f2, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2> Fire(EventDetail.EventCategory category, string group, string template, params Func<TFact1, TFact2, object>[] parameters )
        {
            Conditions.Add( ( f1, f2 ) => true );
            Consequence = ( f1, f2 ) => _BuildMessage( f1, f2, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> FireAffirm( int weight )
        {
            Conditions.Add( ( f1, f2 ) => true );
            Consequence = ( f1, f2 ) => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> FireVeto( int weight )
        {
            Conditions.Add( ( f1, f2 ) => true );
            Consequence = ( f1, f2 ) => _HandleVeto( weight );
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
        public Rule<TFact1, TFact2> Otherwise( Action<TFact1, TFact2> oConsequence )
        {
            Negated = true;
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> Otherwise( string template, params Func<TFact1, TFact2, object>[] parameters )
        {
            Negated = true;
            Consequence = ( f1, f2 ) => _BuildMessage( f1, f2, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> Otherwise( string group, string template, params Func<TFact1, TFact2, object>[] parameters)
        {
            Negated = true;
            Consequence = (f1, f2) => _BuildMessage(f1, f2, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> Otherwise(EventDetail.EventCategory category, string group, string template, params Func<TFact1, TFact2, object>[] parameters )
        {
            Negated = true;
            Consequence = ( f1, f2 ) => _BuildMessage( f1, f2, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> OtherwiseAffirm( int weight )
        {
            Negated = true;
            Consequence = ( f1, f2 ) => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2> OtherwiseVeto( int weight )
        {
            Negated = true;
            Consequence = ( f1, f2 ) => _HandleVeto( weight );
            return this;
        }


        private void _BuildMessage( TFact1 fact1, TFact2 fact2, EventDetail.EventCategory category, string group, string template, [NotNull] Func<TFact1, TFact2, object>[] parameters )
        {
            int len = parameters.Length;

            var markers = new object[len];
            for( int i = 0; i < len; i++ )
            {
                object o = parameters[i]( fact1, fact2 ) ?? "null";
                markers[i] = o;
            }

            string desc = String.Format( template, markers );
            RuleThreadLocalStorage.CurrentContext.Event( category, group, desc );
        }


        [NotNull]
        public Rule<TFact1, TFact2> Modifies( Func<TFact1, TFact2, object> modifyFunc )
        {
            ModifyFunc = modifyFunc;
            return this;
        }


        protected override IRule InternalEvaluate( [NotNull] object[] offered )
        {

            base.InternalEvaluate( offered );

            var fact1 = (TFact1)offered[0];
            var fact2 = (TFact2)offered[1];

            foreach( var cond in Conditions )
            {
                bool result = cond( fact1, fact2 );

                if( !(result) && !(Negated) )
                    return null;
                else if( result && Negated )
                    return null;
            }

            return this;

        }


        protected override void InternalFire( [NotNull] object[] offered )
        {
            base.InternalFire( offered );

            var fact1 = (TFact1)offered[0];
            var fact2 = (TFact2)offered[1];

            Consequence( fact1, fact2 );

            if( ModifyFunc != null )
            {
                object modified = ModifyFunc( fact1, fact2 );
                if( modified is Array )
                {
                    foreach( object o in (modified as Array) )
                        RuleThreadLocalStorage.CurrentContext.ModifyFact( o );
                }
                else if( (modified != null) )
                    RuleThreadLocalStorage.CurrentContext.ModifyFact( modified );
            }
        }
    }


    public class Rule<TFact1, TFact2, TFact3> : AbstractRule
    {
        public Rule( string setName, string ruleName )
            : base( setName, ruleName )
        {
            Negated = false;

            Conditions = new List<Func<TFact1, TFact2, TFact3, bool>>();
            Consequence = null;
        }

        public bool Negated { get; private set; }


        protected IList<Func<TFact1, TFact2, TFact3, bool>> Conditions { get; set; }
        protected Action<TFact1, TFact2, TFact3> Consequence { get; set; }
        protected Func<TFact1, TFact2, TFact3, object> ModifyFunc { get; set; }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> WithSalience( int value )
        {
            Salience = value;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> InMutex( string name )
        {
            Mutex = name;
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3> WithInception( DateTime inception )
        {
            Inception = inception;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> WithExpiration( DateTime expiration )
        {
            Expiration = expiration;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> FireOnce()
        {
            OnlyFiresOnce = true;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> FireAlways()
        {
            OnlyFiresOnce = false;
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3> If( Func<TFact1, TFact2, TFact3, bool> oCondition )
        {
            Conditions.Add( oCondition );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> And( Func<TFact1, TFact2, TFact3, bool> oCondition )
        {
            Conditions.Add( oCondition );
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3> Then( Action<TFact1, TFact2, TFact3> oConsequence )
        {
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> Then( string template, params Func<TFact1, TFact2, TFact3, object>[] parameters )
        {
            Consequence = ( f1, f2, f3 ) => _BuildMessage( f1, f2, f3, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> Then( string group, string template, params Func<TFact1, TFact2, TFact3, object>[] parameters)
        {
            Consequence = (f1, f2, f3) => _BuildMessage(f1, f2, f3, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }



        [NotNull]
        public Rule<TFact1, TFact2, TFact3> Then(EventDetail.EventCategory category, string group, string template, params Func<TFact1, TFact2, TFact3, object>[] parameters )
        {
            Consequence = ( f1, f2, f3 ) => _BuildMessage( f1, f2, f3, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> ThenAffirm( int weight )
        {
            Consequence = ( f1, f2, f3 ) => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> ThenVeto( int weight )
        {
            Consequence = ( f1, f2, f3 ) => _HandleVeto( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> Fire( Action<TFact1, TFact2, TFact3> oConsequence )
        {
            Conditions.Add( ( f1, f2, f3 ) => true );
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> Fire( string template, params Func<TFact1, TFact2, TFact3, object>[] parameters )
        {
            Conditions.Add( ( f1, f2, f3 ) => true );
            Consequence = ( f1, f2, f3 ) => _BuildMessage( f1, f2, f3, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> Fire( string group, string template, params Func<TFact1, TFact2, TFact3, object>[] parameters)
        {
            Conditions.Add((f1, f2, f3) => true);
            Consequence = (f1, f2, f3) => _BuildMessage(f1, f2, f3, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3> Fire(EventDetail.EventCategory category, string group, string template, params Func<TFact1, TFact2, TFact3, object>[] parameters )
        {
            Conditions.Add( ( f1, f2, f3 ) => true );
            Consequence = ( f1, f2, f3 ) => _BuildMessage( f1, f2, f3, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> FireAffirm( int weight )
        {
            Conditions.Add( ( f1, f2, f3 ) => true );
            Consequence = ( f1, f2, f3 ) => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> FireVeto( int weight )
        {
            Conditions.Add( ( f1, f2, f3 ) => true );
            Consequence = ( f1, f2, f3 ) => _HandleVeto( weight );
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
        public Rule<TFact1, TFact2, TFact3> Otherwise( Action<TFact1, TFact2, TFact3> oConsequence )
        {
            Negated = true;
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> Otherwise( string template, params Func<TFact1, TFact2, TFact3, object>[] parameters )
        {
            Negated = true;
            Consequence = ( f1, f2, f3 ) => _BuildMessage( f1, f2, f3, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3> Otherwise( string group, string template, params Func<TFact1, TFact2, TFact3, object>[] parameters)
        {
            Negated = true;
            Consequence = (f1, f2, f3) => _BuildMessage(f1, f2, f3, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3> Otherwise(EventDetail.EventCategory category, string group, string template, params Func<TFact1, TFact2, TFact3, object>[] parameters )
        {
            Negated = true;
            Consequence = ( f1, f2, f3 ) => _BuildMessage( f1, f2, f3, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> OtherwiseAffirm( int weight )
        {
            Negated = true;
            Consequence = ( f1, f2, f3 ) => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3> OtherwiseVeto( int weight )
        {
            Negated = true;
            Consequence = ( f1, f2, f3 ) => _HandleVeto( weight );
            return this;
        }


        private void _BuildMessage( TFact1 fact1, TFact2 fact2, TFact3 fact3, EventDetail.EventCategory category, string group, string template, [NotNull] Func<TFact1, TFact2, TFact3, object>[] parameters )
        {
            int len = parameters.Length;

            var markers = new object[len];
            for( int i = 0; i < len; i++ )
            {
                object o = parameters[i]( fact1, fact2, fact3 ) ?? "null";
                markers[i] = o;
            }

            string desc = String.Format( template, markers );
            RuleThreadLocalStorage.CurrentContext.Event( category, group, desc );
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3> Modifies( Func<TFact1, TFact2, TFact3, object> modifyFunc )
        {
            ModifyFunc = modifyFunc;
            return this;
        }


        protected override IRule InternalEvaluate( [NotNull] object[] offered )
        {
            base.InternalEvaluate( offered );

            var fact1 = (TFact1)offered[0];
            var fact2 = (TFact2)offered[1];
            var fact3 = (TFact3)offered[2];

            foreach( var cond in Conditions )
            {
                bool bResult = cond( fact1, fact2, fact3 );

                if( !(bResult) && !(Negated) )
                    return null;
                else if( bResult && Negated )
                    return null;
            }

            return this;

        }


        protected override void InternalFire( [NotNull] object[] offered )
        {
            base.InternalFire( offered );

            var fact1 = (TFact1)offered[0];
            var fact2 = (TFact2)offered[1];
            var fact3 = (TFact3)offered[2];

            Consequence( fact1, fact2, fact3 );

            if( ModifyFunc != null )
            {
                object modified = ModifyFunc( fact1, fact2, fact3 );
                if( modified is Array )
                {
                    foreach( object o in (modified as Array) )
                        RuleThreadLocalStorage.CurrentContext.ModifyFact( o );
                }
                else if( (modified != null) )
                    RuleThreadLocalStorage.CurrentContext.ModifyFact( modified );
            }
        }
    }


    public class Rule<TFact1, TFact2, TFact3, TFact4> : AbstractRule
    {
        public Rule( string setName, string ruleName ) : base( setName, ruleName )
        {
            Negated = false;

            Conditions = new List<Func<TFact1, TFact2, TFact3, TFact4, bool>>();
            Consequence = null;
        }

        public bool Negated { get; private set; }


        protected IList<Func<TFact1, TFact2, TFact3, TFact4, bool>> Conditions { get; set; }
        protected Action<TFact1, TFact2, TFact3, TFact4> Consequence { get; set; }
        protected Func<TFact1, TFact2, TFact3, TFact4, object> ModifyFunc { get; set; }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> WithSalience( int value )
        {
            Salience = value;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> InMutex( string name )
        {
            Mutex = name;
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> WithInception( DateTime inception )
        {
            Inception = inception;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> WithExpiration( DateTime expiration )
        {
            Expiration = expiration;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> FireOnce()
        {
            OnlyFiresOnce = true;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> FireAlways()
        {
            OnlyFiresOnce = false;
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> If( Func<TFact1, TFact2, TFact3, TFact4, bool> oCondition )
        {
            Conditions.Add( oCondition );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> And( Func<TFact1, TFact2, TFact3, TFact4, bool> oCondition )
        {
            Conditions.Add( oCondition );
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> Then( Action<TFact1, TFact2, TFact3, TFact4> oConsequence )
        {
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> Then( string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters )
        {
            Consequence = ( f1, f2, f3, f4 ) => _BuildMessage( f1, f2, f3, f4, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> Then(string group, string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters)
        {
            Consequence = (f1, f2, f3, f4) => _BuildMessage(f1, f2, f3, f4, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> Then(EventDetail.EventCategory category, string group, string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters )
        {
            Consequence = ( f1, f2, f3, f4 ) => _BuildMessage( f1, f2, f3, f4, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> ThenAffirm( int weight )
        {
            Consequence = ( f1, f2, f3, f4 ) => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> ThenVeto( int weight )
        {
            Consequence = ( f1, f2, f3, f4 ) => _HandleVeto( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> Fire( Action<TFact1, TFact2, TFact3, TFact4> oConsequence )
        {
            Conditions.Add( ( f1, f2, f3, f4 ) => true );
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> Fire( string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters )
        {
            Conditions.Add( ( f1, f2, f3, f4 ) => true );
            Consequence = ( f1, f2, f3, f4 ) => _BuildMessage( f1, f2, f3, f4, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> Fire( string group, string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters)
        {
            Conditions.Add((f1, f2, f3, f4) => true);
            Consequence = (f1, f2, f3, f4) => _BuildMessage(f1, f2, f3, f4, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> Fire(EventDetail.EventCategory category, string group, string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters )
        {
            Conditions.Add( ( f1, f2, f3, f4 ) => true );
            Consequence = ( f1, f2, f3, f4 ) => _BuildMessage( f1, f2, f3, f4, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> FireAffirm( int weight )
        {
            Conditions.Add( ( f1, f2, f3, f4 ) => true );
            Consequence = ( f1, f2, f3, f4 ) => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> FireVeto( int weight )
        {
            Conditions.Add( ( f1, f2, f3, f4 ) => true );
            Consequence = ( f1, f2, f3, f4 ) => _HandleVeto( weight );
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
        public Rule<TFact1, TFact2, TFact3, TFact4> Otherwise( Action<TFact1, TFact2, TFact3, TFact4> oConsequence )
        {
            Negated = true;
            Consequence = oConsequence;
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> Otherwise( string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters )
        {
            Negated = true;
            Consequence = ( f1, f2, f3, f4 ) => _BuildMessage( f1, f2, f3, f4, EventDetail.EventCategory.Info, "", template, parameters );
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> Otherwise( string group, string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters)
        {
            Negated = true;
            Consequence = (f1, f2, f3, f4) => _BuildMessage(f1, f2, f3, f4, EventDetail.EventCategory.Info, group, template, parameters);
            return this;
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> Otherwise(EventDetail.EventCategory category, string group, string template, params Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters )
        {
            Negated = true;
            Consequence = ( f1, f2, f3, f4 ) => _BuildMessage( f1, f2, f3, f4, category, group, template, parameters );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> OtherwiseAffirm( int weight )
        {
            Negated = true;
            Consequence = ( f1, f2, f3, f4 ) => _HandleAffirm( weight );
            return this;
        }

        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> OtherwiseVeto( int weight )
        {
            Negated = true;
            Consequence = ( f1, f2, f3, f4 ) => _HandleVeto( weight );
            return this;
        }


        private void _BuildMessage( TFact1 fact1, TFact2 fact2, TFact3 fact3, TFact4 fact4, EventDetail.EventCategory category, string group, string template, [NotNull] Func<TFact1, TFact2, TFact3, TFact4, object>[] parameters )
        {
            int len = parameters.Length;

            var markers = new object[len];
            for( int i = 0; i < len; i++ )
            {
                object o = parameters[i]( fact1, fact2, fact3, fact4 ) ?? "null";
                markers[i] = o;
            }

            string desc = String.Format( template, markers );
            RuleThreadLocalStorage.CurrentContext.Event( category, group, desc );
        }


        [NotNull]
        public Rule<TFact1, TFact2, TFact3, TFact4> Modifies( Func<TFact1, TFact2, TFact3, TFact4, object> modifyFunc )
        {
            ModifyFunc = modifyFunc;
            return this;
        }


        protected override IRule InternalEvaluate( [NotNull] object[] offered )
        {
            base.InternalEvaluate( offered );

            var fact1 = (TFact1)offered[0];
            var fact2 = (TFact2)offered[1];
            var fact3 = (TFact3)offered[2];
            var fact4 = (TFact4)offered[3];

            foreach( var cond in Conditions )
            {
                bool bResult = cond( fact1, fact2, fact3, fact4 );

                if( !(bResult) && !(Negated) )
                    return null;
                else if( bResult && Negated )
                    return null;
            }

            return this;

        }


        protected override void InternalFire( [NotNull] object[] offered )
        {
            base.InternalFire( offered );

            var fact1 = (TFact1)offered[0];
            var fact2 = (TFact2)offered[1];
            var fact3 = (TFact3)offered[2];
            var fact4 = (TFact4)offered[3];


            Consequence( fact1, fact2, fact3, fact4 );

            if( ModifyFunc != null )
            {
                object modified = ModifyFunc( fact1, fact2, fact3, fact4 );
                if( modified is Array )
                {
                    foreach( object o in (modified as Array) )
                        RuleThreadLocalStorage.CurrentContext.ModifyFact( o );
                }
                else if( (modified != null) )
                    RuleThreadLocalStorage.CurrentContext.ModifyFact( modified );
            }
        }
    }

}
