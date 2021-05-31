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
using Fabrica.Rules.Evaluation;
using JetBrains.Annotations;

namespace Fabrica.Rules.Builder
{

    public abstract class AbstractRule : IRule
    {

        protected AbstractRule( string fqNamespace, string ruleName )
        {
            Namespace = fqNamespace;
            Name = $"{Namespace}.{ruleName}";

            Salience = 500;
            Mutex = "";

            OnlyFiresOnce = false;

            Inception = DateTime.MinValue;
            Expiration = DateTime.MaxValue;
        }


        public string Namespace { get; set; }
        public string Name { get; set; }

        public int Salience { get; protected set; }

        public bool OnlyFiresOnce { get; protected set; }

        public string Mutex { get; protected set; }

        public DateTime Inception { get; set; }
        public DateTime Expiration { get; set; }


        public IRule EvaluateRule( object[] offered )
        {
            return InternalEvaluate( offered );
        }

        public void FireRule( object[] offered )
        {
            InternalFire( offered );
        }


        [CanBeNull]
        protected virtual IRule InternalEvaluate( object[] offered )
        {
            RuleThreadLocalStorage.CurrentContext.Results.TotalEvaluated++;
            return null;
        }

        protected virtual void InternalFire( object[] offered )
        {
            RuleThreadLocalStorage.CurrentContext.Results.TotalFired++;
        }

    }

}
