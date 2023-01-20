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

using System.Reflection;

namespace Fabrica.Utilities.Types;

public class TypeSource
{

    private static Func<Type, bool> DefaultPredicate { get; } = _=>true;

    protected virtual Func<Type, bool> GetPredicate()
    {
        return DefaultPredicate;
    }


    public void AddTypes( params Assembly[] assemblies )
    {

        if (assemblies == null) throw new ArgumentNullException(nameof(assemblies));

        foreach ( var type in assemblies.SelectMany(a=>a.GetTypes()).Where(GetPredicate()) )
            Types.Add(type);
    }


    public void AddTypes( params Type[] types )
    {

        if (types == null) throw new ArgumentNullException(nameof(types));

        foreach (var type in types.Where(GetPredicate()))
            Types.Add(type);
    }


    public void AddTypes( IEnumerable<Type> candidates )
    {

        if (candidates == null) throw new ArgumentNullException(nameof(candidates));

        foreach (var type in candidates.Where( GetPredicate() ) )
            Types.Add(type);
    }


    private HashSet<Type> Types { get; } = new ();

    public IEnumerable<Type> GetTypes()
    {
        return Types;
    }


}