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

using Fabrica.Exceptions;
using Fabrica.Rules.Builder;
using Fabrica.Rules.Evaluation;
using Fabrica.Watch;

namespace Fabrica.Rules.Listeners
{


    public class LoggingEvaluationListener: IEvaluationListener
    {

        public LoggingEvaluationListener( ILogger logger )
        {
            Logger = logger;
        }

        public LoggingEvaluationListener(  string category )
        {
            Logger = WatchFactoryLocator.Factory.GetLogger( category );
        }


        private ILogger Logger { get; }


        public void BeginEvaluation()
        {

            if( !(Logger.IsDebugEnabled) )
                return;

            var context = RuleThreadLocalStorage.CurrentContext;            

            Logger.EnterScope( $"Begin Evaluation - ({context.Description})" );

            Logger.LogObject( "Context", context );

        }

        public void BeginTupleEvaluation( object[] facts )
        {

            if (!(Logger.IsDebugEnabled))
                return;

            Logger.EnterScope( "Begin Tuple Evaluation" );

            for (int i = 0; i < facts.Length; i++)
                Logger.LogObject($"{facts[i].GetType().FullName}[{i}]", facts[i]);


        }

        public void FiringRule( IRule rule )
        {

            if (!(Logger.IsDebugEnabled))
                return;

            Logger.LogObject($"Rule Firing ({rule.Name})", rule );

        }

        public void FiredRule( IRule rule, bool modified )
        {

            if (!(Logger.IsDebugEnabled))
                return;

            Logger.Debug( "Rule Fired ({0}). Modified fact? {1}", rule.Name, modified );

        }

        public void EndTupleEvaluation( object[] facts )
        {

            if (!(Logger.IsDebugEnabled))
                return;

            Logger.LeaveScope( "End Tuple Evaluation" );

        }

        public void EndEvaluation()
        {

            if (!(Logger.IsDebugEnabled))
                return;

            var context = RuleThreadLocalStorage.CurrentContext;                                    

            Logger.LogObject( "Results", context.Results );

            Logger.LeaveScope( $"End Evaluation - ({context.Description})" );

        }

        public void EventCreated( EventDetail evalEvent )
        {

            if (!(Logger.IsDebugEnabled))
                return;

            Logger.LogObject( "Evaluation Event Created", evalEvent );

        }

        public void Debug( string template, params object[] markers )
        {

            if (!(Logger.IsDebugEnabled))
                return;

            Logger.Debug( template, markers );

        }

        public void Warning( string template, params object[] markers )
        {

            if (!(Logger.IsWarningEnabled))
                return;
            
            Logger.Warning( template, markers );

        }

    }

}
