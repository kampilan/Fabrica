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
using Fabrica.Rules.Evaluation;
using Fabrica.Rules.Listeners;
using Fabrica.Rules.Util;
using JetBrains.Annotations;

namespace Fabrica.Rules
{
    public class EvaluationContext
    {
        public EvaluationContext()
        {
            ThrowValidationException = true;
            ThrowNoRulesException = true;

            Listener = new NoopEvaluationListener();

            Space = new FactSpace();

            Tables = new Dictionary<string, IDictionary<object, object>>();

            Results = new EvaluationResults();

            CurrentRuleName = "";
            ModificationsOccurred = false;
            InsertionsOccurred = false;

            Description = "";

            MaxEvaluations = 500000;
            MaxDuration = 10*1000;

            MaxViolations = int.MaxValue;

            Mutexed = new HashSet<string>();
            FireOnceRules = new Dictionary<object, ISet<long>>();

        }


        internal FactSpace Space { get; }

        internal string CurrentRuleName { get; set; }

        internal long CurrentIdentity { get; set; }
        internal long CurrentSelector { get; set; }
        internal object[] CurrentTuple { get; set; }


        internal bool ModificationsOccurred { get; private set; }
        internal bool InsertionsOccurred { get; private set; }

        internal ISet<string> Mutexed { get; }
        internal IDictionary<object, ISet<long>> FireOnceRules { get; set; }


        internal bool IsExhausted => (Results.TotalEvaluated > MaxEvaluations) || ((DateTime.Now - Results.Started).TotalMilliseconds > MaxDuration);


        public string Description { get; set; }

        public bool ThrowValidationException { get; set; }
        public bool ThrowNoRulesException { get; set; }

        public IEvaluationListener Listener { get; set; }
        public EvaluationResults Results { get; }

        public int MaxEvaluations { get; set; }
        public long MaxDuration { get; set; }


        public int MaxViolations { get; set; }

        internal bool ViolationsExceeded
        {
            get { return Results.Events.Count( e => e.Category == EventDetail.EventCategory.Violation ) >= MaxViolations; }
        }



        private IDictionary<string, IDictionary<object, object>> Tables { get; }

        public void AddLookup<TMember>(Func<TMember,object> keyExtractor, IEnumerable<TMember> members )
        {
            var name = typeof(TMember).FullName;
            AddLookup( name, keyExtractor, members );
        }

        public void AddLookup<TMember>( string name, Func<TMember, object> keyExtractor, [NotNull] IEnumerable<TMember> members )
        {

            IDictionary<object,object> table = new Dictionary<object, object>();

            foreach( var m in members )
            {
                object key = keyExtractor( m );
                table[key] = m;
            }

            Tables[name] = table;

        }

        public void AddLookup( string name, IDictionary<object, object> table )
        {
            Tables[name] = table;            
        }



        public TMember Lookup<TMember>( object key )
        {
            string name = typeof( TMember ).FullName;
            return Lookup<TMember>( name, key );
        }

        [NotNull]
        public TMember Lookup<TMember>( string name, object key )
        {
            if( !(Tables.TryGetValue( name, out var table )) )
                throw new InvalidOperationException($"Could not find lookup table with the name {name}");

            if( !(table.TryGetValue( key, out var member )) )
                throw new InvalidOperationException($"Could not find member using key {key} from table {name}");

            if( !(member is TMember) )
                throw new InvalidOperationException( $"Could not cast member to type {typeof (TMember).FullName} using key {key} from table {name}" );

            return (TMember)member;

        }



        public IDictionary<string, object> Shared => Results.Shared;

        internal void InsertFact( [NotNull] object fact )
        {
            if( fact == null )
                throw new ArgumentNullException( nameof(fact) );

            int index = _SelectorFromFact( fact );
            if( index == 0 )
            {
                Space.InsertFact( fact );
                InsertionsOccurred = true;
            }
        }


        internal void ModifyFact( [NotNull] object fact )
        {
            if( fact == null )
                throw new ArgumentNullException( nameof(fact) );

            int index = _SelectorFromFact( fact );
            if( index > 0 )
            {
                Space.ModifyFact( index );
                ModificationsOccurred = true;
            }
        }


        internal void RetractFact( [NotNull] object fact )
        {
            if( fact == null )
                throw new ArgumentNullException( nameof(fact) );

            int index = _SelectorFromFact( fact );
            if( index > 0 )
            {
                Space.RetractFact( index );
                ModificationsOccurred = true;
            }
        }

        private int _SelectorFromFact( object fact )
        {
            var selectorIndices = Helpers.DecodeSelector( CurrentSelector );

            var len = selectorIndices.Length;

            for( var i = 0; i < len; i++ )
                if( fact == CurrentTuple[i] )
                    return selectorIndices[i];

            return 0;
        }

        internal void ResetBetweenTuples()
        {
            InsertionsOccurred = false;
        }


        internal void ResetBetweenRules()
        {
            CurrentRuleName = "";
            ModificationsOccurred = false;
        }

        internal virtual void Event( EventDetail.EventCategory category, string group, [NotNull] string template, params object[] parameters )
        {

            
            if( template == null )
                throw new ArgumentNullException( nameof(template) );

            if( string.IsNullOrWhiteSpace( template ) )
                throw new InvalidOperationException( "Can not create an Event with a blank message (template was null or blank)" );


            var source = "";
            if (CurrentTuple.Length > 0)
                source = CurrentTuple[0].ToString();


            var theEvent = new EventDetail
            {
                Category    = category,
                Source      = source,
                Group       = group,
                RuleName    = CurrentRuleName,
                Explanation = string.Format( template, parameters )
            };

            Results.Events.Add( theEvent );

        }


        public void AddFacts( params object[] facts )
        {
            Space.Add( facts );
        }


        public void AddAllFacts( IEnumerable<object> facts )
        {
            Space.AddAll( facts );
        }


    }

}
