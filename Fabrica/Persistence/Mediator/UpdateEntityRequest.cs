﻿using System.Reflection;
using System.Text.Json.Serialization;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using JetBrains.Annotations;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class UpdateEntityRequest<TEntity>: BaseEntityRequest, IRequest<Response<TEntity>>, IUpdateEntityRequest where TEntity: class, IModel
{

    public string Uid { get; set; } = "";

    public Dictionary<string, object> Delta { get; set; } = new();

    public void FromObject([NotNull] object source)
    {

        if (source == null) throw new ArgumentNullException(nameof(source));

        foreach (var pi in source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead))
        {
            var value = pi.GetValue(source, null);
            if (value is not null)
                Delta[pi.Name] = value;
        }

    }

    [JsonIgnore]
    public Func<IMessageMediator, Task<IResponse>> Sender => async (m) => await m.Send(this);

}