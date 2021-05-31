﻿/*
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

namespace Fabrica.Rules.Listeners
{

    public class NoopEvaluationListener : IEvaluationListener
    {
        public void BeginEvaluation()
        {
        }

        public void BeginTupleEvaluation( object[] facts )
        {
        }

        public void FiringRule( IRule rule )
        {
        }

        public void FiredRule( IRule rule, bool modified )
        {
        }

        public void EndTupleEvaluation( object[] facts )
        {
        }

        public void EndEvaluation()
        {
        }

        public void Debug( string template, params object[] markers )
        {
        }

        public void Warning( string template, params object[] markers )
        {
        }

        public void EventCreated( EventDetail evalEvent )
        {
        }

    }

}
