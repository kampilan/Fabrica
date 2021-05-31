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
using Fabrica.Rules.Evaluation;
using JetBrains.Annotations;

namespace Fabrica.Rules.Builder
{

    internal class SubRule<TParent,TFact>: IRule
    {

        public SubRule( IEnumerable<TFact> facts, Action<TFact> consequence, TParent parent, Func<TParent,object> modifyFunc  )
        {
            Facts = facts;
            Consequence = consequence;

            Parent = parent;
            ModifyFunc = modifyFunc;

        }

        private IEnumerable<TFact> Facts { get;}
        private Action<TFact> Consequence { get;}

        private TParent Parent { get; }
        private Func<TParent, object> ModifyFunc { get;}

        public string Namespace { get; set; }
        public string Name { get; set; }
        public int Salience { get; set; }
        public bool OnlyFiresOnce { get; set; }
        public string Mutex { get; set; }
        public DateTime Inception { get; set; }
        public DateTime Expiration { get; set; }

        [NotNull]
        public IRule EvaluateRule( object[] fact )
        {
            return this;
        }

        public void FireRule( object[] offered )
        {

            foreach( var fact in Facts )
                Consequence( fact );

            if (ModifyFunc != null)
            {
                var modified = ModifyFunc( Parent );
                if( modified is Array )
                {
                    foreach (var o in (modified as Array))
                        RuleThreadLocalStorage.CurrentContext.ModifyFact( o );
                }
                else if ((modified != null))
                    RuleThreadLocalStorage.CurrentContext.ModifyFact( modified );
            }

            RuleThreadLocalStorage.CurrentContext.Results.TotalFired++;

        }

    }

}
