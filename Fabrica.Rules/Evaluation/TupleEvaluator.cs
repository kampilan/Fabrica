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

using Fabrica.Rules.Builder;

namespace Fabrica.Rules.Evaluation;

internal class TupleEvaluator
{
    public TupleEvaluator( ISet<IRule> rules )
    {
        Rules = rules;
    }

    private ISet<IRule> Rules { get; }

    private EvaluationContext Context => RuleThreadLocalStorage.CurrentContext;

    public void Evaluate( object[] tuple )
    {

        Context.Listener.BeginTupleEvaluation( tuple );

        Context.ResetBetweenTuples();

        var fireableRules = _GetFireableRules( tuple );

        foreach( var rule in fireableRules )
        {
            // Ignore rules that were excluded by a mutex
            // This check is also made in _Filter. A mutex may have occurred
            // during the processing of this fire list
            if( (rule.Mutex != "") && (Context.Mutexed.Contains( rule.Mutex )) )
                continue;


            Context.ResetBetweenRules();


            // Handle Mutually exclusive rules
            // This rule is the winner in the exclusion
            // No other rule in this mutex will be called
            // for this tuple instance
            if( rule.Mutex != "" )
                _HandleMutxedRule( rule );


            Context.CurrentRuleName = rule.Name;

            Context.Listener.FiringRule( rule );

            RuleThreadLocalStorage.CurrentContext.Results.TotalFired++;
            rule.FireRule( tuple );


            // Handle fire once rules. Some rules should only 
            // be fired once per tuple instance.
            if( rule.OnlyFiresOnce )
                _HandleFireOnceRule( rule );


            // Add this rule to the dictionary of fired rules. Useful for auditing and debugging
            if( !(Context.Results.FiredRules.ContainsKey( rule.Name )) )
                Context.Results.FiredRules[rule.Name] = 0;

            // Increment the fire count
            Context.Results.FiredRules[rule.Name]++;


            Context.Listener.FiredRule( rule, Context.ModificationsOccurred );


            // Check that the MaxViolation limit has not been exceeded
            // Stop evaluating if it has
            if( Context.ViolationsExceeded )
                break;

            // Check for exhustion by either time or evaluation count
            if( Context.IsExhausted )
                break;

            // If the rule signaled that it modified the fact
            // We need to re-evaluate and see if this change resulted 
            // in new rules being fired. 
            if( Context.ModificationsOccurred )
                break;
        }


        Context.Listener.EndTupleEvaluation( tuple );

    }


    private void _HandleMutxedRule(  IRule rule )
    {
        Context.Mutexed.Add( rule.Mutex );
        Context.Results.MutexWinners[rule.Mutex] = rule.Name;
    }

    private void _HandleFireOnceRule( IRule rule )
    {

        if ( !(Context.FireOnceRules.TryGetValue( rule, out var set )) )
        {
            set = new HashSet<long>();
            Context.FireOnceRules[rule] = set;
        }

        set.Add( Context.CurrentIdentity );
    }


        
    private IEnumerable<IRule> _GetFireableRules( object[] facts )
    {
        IEnumerable<IRule> rules = Rules.Select( r => _Filter( r, facts ) ).Where( r => r != null ).OrderBy( r => r.Salience );
        return rules;
    }

    private IRule _Filter(  IRule rule, object[] facts )
    {


        // Ignore rules that are prior to inception or after expiration
        if ((rule.Inception > Context.Results.Started) || (rule.Expiration < Context.Results.Started))
            return null;

        // Ignore rules that were exluded by a mutex
        if ((rule.Mutex != "") && (Context.Mutexed.Contains(rule.Mutex)))
            return null;

        if (rule.OnlyFiresOnce)
        {
            // Ignore rules that are fire once only and have already been fired
            if (Context.FireOnceRules.TryGetValue(rule, out var identities))
            {
                if (identities.Contains(Context.CurrentIdentity))
                    return null;

            }
        }

        RuleThreadLocalStorage.CurrentContext.Results.TotalEvaluated++;
        var result = rule.EvaluateRule(facts);

        return result;


    }

}