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
using Fabrica.Rules.Exceptions;
using Fabrica.Rules.Tree;
using Fabrica.Rules.Util;

namespace Fabrica.Rules.Evaluation
{

    public abstract class AbstractEvaluator : IEvaluator
    {

        public EvaluationResults Evaluate( params object[] facts )
        {
            return EvaluateAll( facts );
        }

        public EvaluationResults EvaluateAll( IEnumerable<object> facts )
        {
            var context = BuildContext();
            context.Space.AddAll( facts );

            return Evaluate( context );
        }

        
        public EvaluationResults Evaluate( EvaluationContext evc )
        {
            if( evc == null )
                throw new ArgumentNullException( nameof(evc) );

            var context = evc;    


            //**************************************************
            // Prepare the context for evaluation
            //**************************************************
            var plan = new EvaluationPlan( GetRuleBase(), GetNamespaces(), context.Space );
            plan.Build();

            RuleThreadLocalStorage.CurrentContext = context;

            context.Results.Started = DateTime.Now;

            context.Listener.BeginEvaluation();

            //**************************************************

            try
            {
                int lastSignature = 0;
                TupleEvaluator evaluator = null;

                EvaluationStep nextStep;
                do
                {
                    nextStep = plan.Next();

                    if( nextStep.Signature != 0 )
                    {
                        // If the tuple signature has not changed
                        // the existing evaluator engine can be used
                        // given that the facts are processed
                        // in order by the signature this is a big 
                        // optimization
                        if( nextStep.Signature != lastSignature )
                        {
                            evaluator = null;
                            lastSignature = nextStep.Signature;
                        }

                        // Decode the selector into the indices
                        // Each indices points to the required
                        // fact in the factspace
                        int[] selectorIndices = Helpers.DecodeSelector( nextStep.Selector );
                        object[] currentTuple = context.Space.GetTuple( selectorIndices );


                        // If space returns an empty tuple
                        // it signals that the given step
                        // was eliminated by modification
                        // so skip it
                        if( currentTuple.Length == 0 )
                            continue;


                        if( evaluator == null )
                        {
                            var factTypes = new Type[currentTuple.Length];
                            for( int i = 0; i < factTypes.Length; i++ )
                                factTypes[i] = currentTuple[i].GetType();

                            ISet<IRule> rules = GetRuleBase().FindRules( factTypes, GetNamespaces() );
                            evaluator = new TupleEvaluator( rules );
                        }


                        // Setup the context with the current selector and the current identity
                        // translated from the selectorIndices for this tuple
                        long identity = Helpers.EncodeSelector( context.Space.GetIdentityFromSelector( selectorIndices ) );

                        context.CurrentIdentity = identity;
                        context.CurrentSelector = nextStep.Selector;
                        context.CurrentTuple = currentTuple;

                        // Finally evaluate this tuple against the rules for this signature
                        evaluator.Evaluate( currentTuple );


                        // Stop if the Maximum allowed violation count has been met for excceded
                        if( context.ViolationsExceeded )
                            break;

                        // Stop if Exhustion has occurred
                        if (context.IsExhausted)
                            break;

                        // If there was a modification (InsertFact,ModifiyFact,RetractFact)
                        // Re-build the execution plan to account for the changes
                        if (!context.ModificationsOccurred && !context.InsertionsOccurred)
                            continue;


                        plan.Modify();


                    }
                } while( nextStep.Signature != 0 );
            }
            finally
            {

                // Stop the evaluation timer
                context.Results.Completed = DateTime.Now;

                context.Listener.EndEvaluation();
                
                // Clear the current context from TLS
                RuleThreadLocalStorage.ClearCurrentContext();

            }


            // Exhustion has occured due to excessive time or evaluation count
            if( context.IsExhausted )
                throw new EvaluationExhaustedException( context.Results );


            // Throw an exception if no rules were evaluated
            // Throwing the exception is the default behavior
            if( (context.ThrowNoRulesException) && (context.Results.TotalEvaluated == 0) )
                throw new NoRulesEvaluatedException( context.Results );

            // Throw an exception if the events includes any violations
            // Throwing the exception is the default behavior
            if( (context.ThrowValidationException) && (context.Results.HasViolations) )
                throw new ViolationsExistException( context.Results );

            return context.Results;
        }

        protected abstract IRuleBase GetRuleBase();

        protected abstract IEnumerable<string> GetNamespaces();

        
        protected virtual EvaluationContext BuildContext()
        {
            return new EvaluationContext();
        }
    }

}
