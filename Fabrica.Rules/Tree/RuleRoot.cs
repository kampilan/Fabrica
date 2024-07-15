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

namespace Fabrica.Rules.Tree;

internal class RuleRoot
{

    protected ISet<RuleNode> Trunk { get; } = new HashSet<RuleNode>();

    public void Clear()
    {
        foreach( var n in Trunk )
            n.Clear();
    }

    public void Add( Type[] targetTypes, IEnumerable<IRule> rules )
    {
        _RecurseAdd( 0, targetTypes, Trunk, rules );
    }

    private void _RecurseAdd( int depth,  Type[] types, ISet<RuleNode> trunk, IEnumerable<IRule> rules )
    {
        var target = types[depth];

        var node = trunk.Where( n => n.Target == target ).DefaultIfEmpty( new RuleNode() ).First();
        if( node.Target == null )
        {
            node.Target = types[depth];
            trunk.Add( node );
        }

        depth++;
        if( depth == types.Length )
        {
            node.AddRules( rules );
            return;
        }

        _RecurseAdd( depth, types, node.Branches, rules );
    }

    public bool HasRules( Type[] requestTypes, IEnumerable<string> namespaces )
    {
        return _RecurseQuery( 0, requestTypes, Trunk, namespaces, null );
    }

        
    public ISet<IRule> FindRules( Type[] requestTypes )
    {
        ISet<IRule> sink = new HashSet<IRule>();

        _RecurseQuery( 0, requestTypes, Trunk, new string[] {}, sink );

        return sink;
    }

    private bool _RecurseQuery( int depth,  Type[] types, IEnumerable<RuleNode> trunk, IEnumerable<string> namespaces, ISet<IRule> sink )
    {
        var request = types[depth];

        var branches = trunk.Where( n => n.Target.IsAssignableFrom( request ) );

        // increment the depth for the next recursion
        depth++;

        // If we are at the terminus for the given set of types
        // Collect all the rules from these "leaves" and traverse back up the stack
        if( depth == types.Length )
        {
            if( sink != null )
            {
                foreach( RuleNode node in branches )
                    sink.UnionWith( node.Rules );
                return true;
            }
            else
                return branches.Any( node => node.HasRules( namespaces ) );
        }

        return _RecurseQuery( depth, types, branches.SelectMany( n => n.Branches ), namespaces, sink );
    }
}