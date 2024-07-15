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

internal class RuleNode
{
    private ISet<RuleNode> _branches;

    private bool _hasRules;


    private ISet<string> _namespaces;

    private ISet<IRule> _rules;
    public Type Target { get; set; }

        
    public ISet<RuleNode> Branches => _branches ?? (_branches = new HashSet<RuleNode>());

        
    private ISet<string> Namespaces => _namespaces ?? (_namespaces = new HashSet<string>());

        
    public ISet<IRule> Rules => _rules ?? (_rules = new HashSet<IRule>());

    public bool HasRules(  IEnumerable<string> namespaces )
    {
        if( namespaces == null )
            throw new ArgumentNullException( nameof(namespaces) );

        if( !(_hasRules) )
            return false;

        var ns = namespaces.ToList();

        return ns.Count == 0 || ns.Any( n => _namespaces.Contains( n ) );

    }

    public void AddRules(  IEnumerable<IRule> source )
    {
        if( source == null )
            throw new ArgumentNullException( nameof(source) );

        _hasRules = true;
        foreach( IRule r in source )
        {
            Namespaces.Add( r.Namespace );
            Rules.Add( r );
        }
    }


    public void Clear()
    {
        if( _hasRules )
        {
            Namespaces.Clear();
            Rules.Clear();

            _hasRules = false;
        }


        foreach( RuleNode n in Branches )
            n.Clear();

        Branches.Clear();
    }
}