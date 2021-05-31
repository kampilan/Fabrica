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
using Fabrica.Rules.Evaluation;
using Fabrica.Rules.Tree;

namespace Fabrica.Rules.Builder
{

    public abstract class AbstractRuleBuilder
    {

        protected AbstractRuleBuilder()
        {
            SetName = GetType().Name;
        }

        public string SetName { get; protected set; }

        public bool DefaultFireOnce { get; protected set; } = false;
        public int DefaultSalience { get; protected set; } = 500;

        public DateTime DefaultInception { get; protected set; } = DateTime.MinValue;
        public DateTime DefaultExpiration { get; protected set; } = DateTime.MaxValue;


        protected Type[] Targets { get; set; } = new Type[0];

        public ISet<IRule>             Rules { get;  } = new HashSet<IRule>();
        public ISet<Action<IRuleSink>> Sinks { get;  } = new HashSet<Action<IRuleSink>>();

        public virtual void LoadRules( IRuleSink ruleSink )
        {

            foreach( var sink in Sinks )
                sink( ruleSink );

            if( Targets.Length > 0 )
                ruleSink.Add( Targets, Rules );

        }


        protected string CurrentRuleName => RuleThreadLocalStorage.CurrentContext.CurrentRuleName;

        protected TMember Lookup<TMember>( object key )
        {
            return RuleThreadLocalStorage.CurrentContext.Lookup<TMember>( key );
        }

        protected TMember Lookup<TMember>( string name, object key )
        {
            return RuleThreadLocalStorage.CurrentContext.Lookup<TMember>( name, key );
        }

        protected IDictionary<string, object> Shared => RuleThreadLocalStorage.CurrentContext.Shared;

        protected void Info( string group, string template, params object[] markers )
        {
            RuleThreadLocalStorage.CurrentContext.Event( EventDetail.EventCategory.Info, group, template, markers );
        }

        protected void Violation( string group, string template, params object[] markers )
        {
            RuleThreadLocalStorage.CurrentContext.Event( EventDetail.EventCategory.Violation, group, template, markers );
        }

        protected void Affirm( int amount )
        {
            RuleThreadLocalStorage.CurrentContext.Results.Affirm( amount );
        }

        protected void Veto( int amount )
        {
            RuleThreadLocalStorage.CurrentContext.Results.Veto( amount );
        }

        protected void Debug( string template, params object[] markers )
        {
            RuleThreadLocalStorage.CurrentContext.Listener.Debug( template, markers );
        }

        protected void Insert( object fact )
        {
            RuleThreadLocalStorage.CurrentContext.InsertFact( fact );
        }

        protected void Modify( object fact )
        {
            RuleThreadLocalStorage.CurrentContext.ModifyFact( fact );
        }

        protected void Retract( object fact )
        {
            RuleThreadLocalStorage.CurrentContext.RetractFact( fact );
        }
    }

}
