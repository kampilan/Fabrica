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

public class RuleTree : IRuleBase, IRuleSink
{

    private IDictionary<int, RuleRoot> RootMap { get; } = new Dictionary<int, RuleRoot>();

    public int MaxAxisCount => RootMap.Count > 0 ? RootMap.Keys.Max() : 0;

    public bool HasRules(  Type[] factTypes )
    {

        if (factTypes == null)
            throw new ArgumentNullException( nameof(factTypes) );
            
        return HasRules( factTypes, new string[] {} );
    }

    public bool HasRules(  Type[] factTypes, IEnumerable<string> namespaces )
    {

        if( factTypes == null )
            throw new ArgumentNullException( nameof(factTypes) );

        if( namespaces == null )
            throw new ArgumentNullException( nameof(namespaces) );

        RootMap.TryGetValue( factTypes.Length, out var targetRoot );
        if( targetRoot == null )
            return false;

        return targetRoot.HasRules( factTypes, namespaces );

    }


        
    public ISet<IRule> FindRules( Type[] factTypes )
    {

        if (factTypes == null)
            throw new ArgumentNullException( nameof(factTypes) );

        RootMap.TryGetValue( factTypes.Length, out var targetRoot );
        return targetRoot == null ? new HashSet<IRule>() : targetRoot.FindRules( factTypes );
    }

        
    public ISet<IRule> FindRules( Type[] factTypes, IEnumerable<string> namespaces )
    {

        if (factTypes == null)
            throw new ArgumentNullException( nameof(factTypes) );

        if (namespaces == null)
            throw new ArgumentNullException( nameof(namespaces) );
            
            
        var allRules = FindRules( factTypes );


        var ns = namespaces.ToList();
        // If no namespaces were specified return all the rules
        if( ns.Count == 0 )
            return allRules;

        // Otherwise return only rules in the requested namespaces
        ISet<IRule> rulesForSets = new HashSet<IRule>( allRules.Where( r => ns.Contains( r.Namespace ) ) );
        return rulesForSets;
    }


    public void Add( Type factType, IRule rule )
    {
        if (!(RootMap.TryGetValue(1, out var targetRoot)))
        {
            targetRoot = new RuleRoot();
            RootMap[1] = targetRoot;
        }

        targetRoot.Add( new []{factType}, new []{rule});            

    }


    public void Add(  Type[] factTypes, IEnumerable<IRule> rules )
    {
        var axisCount = factTypes.Length;

        if( !(RootMap.TryGetValue( axisCount, out var targetRoot )) )
        {
            targetRoot = new RuleRoot();
            RootMap[axisCount] = targetRoot;
        }

        targetRoot.Add( factTypes, rules );
    }


    public void Clear()
    {
        foreach( var r in RootMap.Values )
            r.Clear();
    }
}