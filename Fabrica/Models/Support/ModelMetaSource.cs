using System;
using System.Reflection;
using Fabrica.Utilities.Types;

namespace Fabrica.Models.Support;

public class ModelMetaSource: TypeSource
{

    private static Func<Type, bool> Predicate { get; } = t =>
    {

        if( typeof(IModel).IsAssignableFrom(t) && t.GetCustomAttribute<ModelAttribute>() is { } attr )
            return !attr.Ignore;

        if (typeof(IModel).IsAssignableFrom(t))
            return true;

        return false;

    };

    protected override Func<Type, bool> GetPredicate()
    {
        return Predicate;
    }

}