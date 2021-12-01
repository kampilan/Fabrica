using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using JetBrains.Annotations;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class DeltaEntityRequest<TEntity>: IDeltaEntityRequest, IRequest<Response<TEntity>> where TEntity: class, IModel
{

    public OperationType Operation { get; set; }

    public string Uid { get; set; } = "";

    public Dictionary<string,object> Delta { get; set; } = new();

    public void FromDelta( [NotNull] BaseDelta source )
    {

        if (source == null) throw new ArgumentNullException(nameof(source));

        foreach( var pi in source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead) )
        {
            var value = pi.GetValue( source, null );
            if (value is not null)
                Delta[pi.Name] = value;
        }

    }

}