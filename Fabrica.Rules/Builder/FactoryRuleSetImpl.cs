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

using System.Collections.Generic;
using Fabrica.Rules.Evaluation;
using Fabrica.Rules.Tree;

namespace Fabrica.Rules.Builder
{

    internal class FactoryRuleSetImpl : AbstractRuleSet
    {
        internal FactoryRuleSetImpl( IRuleBase ruleBase, IEnumerable<string> namespaces, IEvaluationContextFactory contextFactory )
        {
            RuleBase = ruleBase;
            Namespaces = namespaces;
            ContextFactory = contextFactory;
        }


        private IRuleBase RuleBase { get;  }


        internal IEnumerable<string> Namespaces { get;  }

        protected override IRuleBase GetRuleBase()
        {
            return RuleBase;
        }

        protected override IEnumerable<string> GetNamespaces()
        {
            return Namespaces;
        }


        private IEvaluationContextFactory ContextFactory { get; }

        public override EvaluationContext GetEvaluationContext()
        {
            return ContextFactory == null ? new EvaluationContext() : ContextFactory.CreateContext();
        }
    }

}
