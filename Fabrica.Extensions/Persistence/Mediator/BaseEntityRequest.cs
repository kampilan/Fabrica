using System;
using System.Collections.Generic;

namespace Fabrica.Persistence.Mediator;

public class BaseEntityRequest
{

    protected Dictionary<string, object> Flags { get; } = new();

    public TType GetFlag<TType>(string name, TType missing = default)
    {

        if (Flags.TryGetValue(name, out var obj))
            return (TType) Convert.ChangeType(obj, typeof(TType));

        return missing;

    }

    public void SetFlag(string name, object value)
    {
        Flags[name] = value;
    }


}