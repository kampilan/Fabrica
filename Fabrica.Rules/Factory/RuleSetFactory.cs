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
using Fabrica.Rules.Evaluation;
using Fabrica.Rules.Listeners;
using Fabrica.Rules.Tree;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.Rules.Factory;

public class RuleSetFactory : IRequiresStart
{


    public IEvaluationContextFactory ContextFactory { get; set; }
    public IEvaluationListenerFactory ListenerFactory { get; set; }

    private IDictionary<string, IEnumerable<string>> CompositeNamespaces { get; } = new Dictionary<string, IEnumerable<string>>();

    public void RegisterCompositeNamespace(string name, IEnumerable<string> namespaces)
    {
        if (!(Started))
            CompositeNamespaces[name] = namespaces;
    }



    private List<IRuleBuilderSource> Sources { get; } = new List<IRuleBuilderSource>();


    public void AddAllSources(IEnumerable<IRuleBuilderSource> sources)
    {
        Sources.AddRange(sources);
    }

    public void AddSources(params IRuleBuilderSource[] sources)
    {
        Sources.AddRange(sources);
    }


    private RuleTree Tree { get; } = new ();

    internal IRuleBase RuleBase => Tree;

    private bool Started { get; set; }


    public Task Start()
    {

        using var logger = this.EnterMethod();

        logger.Inspect(nameof(Started), Started);

        if (Started)
            return Task.CompletedTask;

        Started = true;

        var builders = Sources.SelectMany(s => s.GetTypes()).Select(t => Activator.CreateInstance(t) as IBuilder);

        foreach (var b in builders.Where(b => b != null))
            b.LoadRules(Tree);


        return Task.CompletedTask;


    }

    public void Stop()
    {

        using var logger = this.EnterMethod();


        Tree.Clear();

        Started = false;

    }



    public EvaluationContext BuildContext()
    {
        var context = ContextFactory != null ? ContextFactory.CreateContext() : new EvaluationContext();

        if (ListenerFactory != null)
            context.Listener = ListenerFactory.CreateListener();

        return context;
    }


    
    public IRuleSet GetRuleSet()
    {
        var ruleSet = new FactoryRuleSetImpl(Tree, new HashSet<string>(), ContextFactory);
        return ruleSet;
    }


    
    public IRuleSet GetRuleSetForComposite(string name)
    {
        if (!(CompositeNamespaces.TryGetValue(name, out var composite)))
            throw new InvalidOperationException($"Could not find Composite Namespace for given name ({name}).");

        var ruleSet = new FactoryRuleSetImpl(Tree, composite, ContextFactory);
        return ruleSet;
    }


    
    public IRuleSet GetRuleSet(params string[] namespaces)
    {
        var ruleSet = new FactoryRuleSetImpl(Tree, new HashSet<string>(namespaces), ContextFactory);
        return ruleSet;
    }


    
    public IRuleSet GetRuleSet(IEnumerable<string> namespaces)
    {
        var ruleSet = new FactoryRuleSetImpl(Tree, new HashSet<string>(namespaces), ContextFactory);
        return ruleSet;
    }


}