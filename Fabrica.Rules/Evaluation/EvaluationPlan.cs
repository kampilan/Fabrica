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

using Fabrica.Rules.Tree;
using Fabrica.Rules.Util;

namespace Fabrica.Rules.Evaluation;

internal struct BuildStats
{
    internal int VariationsConsidered;
    internal int VariationsFound;
    internal int StepsAdded;
}

internal class EvaluationPlan
{
    public EvaluationPlan(  IRuleBase ruleBase, IEnumerable<string> namespaces, FactSpace space )
    {
        RuleBase = ruleBase;
        Space = space;
        Namespaces = namespaces;

        Count = ruleBase.MaxAxisCount;

        Queues = new Queue<EvaluationStep>[Count];
        for( int q = 0; q < Count; q++ )
            Queues[q] = new Queue<EvaluationStep>( 1024 );
    }


    private IRuleBase RuleBase { get; }

    private FactSpace Space { get; }

    private IEnumerable<string> Namespaces { get; }

    private ISet<long> IssuedSelectors { get; } = new HashSet<long>();

    private int Count { get; }
    private Queue<EvaluationStep>[] Queues { get; }


    private IEnumerable<IEnumerable<byte>> _GetVariations( int maxAxis, int typeCount )
    {
        var source = new byte[typeCount];
        for( int i = 0; i < typeCount; i++ )
            source[i] = (byte)i;

        return source.VariationsWithRepetition( maxAxis );
    }


    private int _GenerateSteps(  byte[] typeIndices )
    {
        int stepsAdded = 0;

        int len = typeIndices.Length;

        var factIndices = new IList<int>[len];
        for( int i = 0; i < len; i++ )
        {
            byte typeIndex = typeIndices[i];
            factIndices[i] = Space.Schema[typeIndex].Members;
        }

        Type[] factTypes = Space.GetFactTypes( typeIndices );
        if( !(RuleBase.HasRules( factTypes, Namespaces )) )
            return stepsAdded;

        int signature = Helpers.EncodeSignature( typeIndices );

        if( len == 1 )
        {
            foreach( int p in factIndices[0] )
            {
                _AddStep( signature, new[] {p}, 1 );
                stepsAdded++;
            }
        }
        else if( len == 2 )
        {
            foreach( int p1 in factIndices[0] )
            foreach( int p2 in factIndices[1] )
            {
                _AddStep( signature, new[] {p1, p2}, 2 );
                stepsAdded++;
            }
        }
        else if( len == 3 )
        {
            foreach( int p1 in factIndices[0] )
            foreach( int p2 in factIndices[1] )
            foreach( int p3 in factIndices[2] )
            {
                _AddStep( signature, new[] {p1, p2, p3}, 3 );
                stepsAdded++;
            }
        }
        else if( len == 4 )
        {
            foreach( int p1 in factIndices[0] )
            foreach( int p2 in factIndices[1] )
            foreach( int p3 in factIndices[2] )
            foreach( int p4 in factIndices[3] )
            {
                _AddStep( signature, new[] {p1, p2, p3, p4}, 4 );
                stepsAdded++;
            }
        }

        return stepsAdded;
    }

    private void _AddStep( int signature, int[] indices, int priority )
    {
        long selector = Helpers.EncodeSelector( indices );
        if( IssuedSelectors.Contains( selector ) )
            return;
        else
            IssuedSelectors.Add( selector );

        var step = new EvaluationStep {Priority = priority, Signature = signature, Selector = selector};

        Queues[priority - 1].Enqueue( step );
    }


    internal BuildStats Build()
    {
        var stats = new BuildStats();

        if( Space.TypeCount == 0 )
            return stats;

        int axisCount = RuleBase.MaxAxisCount;

        if( axisCount == 0 )
            return stats;

        foreach( var v in _GetVariations( axisCount, Space.TypeCount ) )
        {
            stats.VariationsConsidered++;
            int stepsAdded = _GenerateSteps( v.ToArray() );

            if( stepsAdded > 0 )
            {
                stats.VariationsFound++;
                stats.StepsAdded += stepsAdded;
            }
        }

        return stats;
    }


    internal void Modify()
    {
        Build();
    }


    internal EvaluationStep Next()
    {
        var next = new EvaluationStep {Signature = 0, Selector = 0};

        for( int q = 0; q < Count; q++ )
        {
            if( Queues[q].Count > 0 )
            {
                next = Queues[q].Dequeue();
                return next;
            }
        }

        return next;
    }
}