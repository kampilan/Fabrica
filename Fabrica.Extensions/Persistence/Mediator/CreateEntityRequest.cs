using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using JetBrains.Annotations;
using MediatR;
using Newtonsoft.Json;

namespace Fabrica.Persistence.Mediator;

public class CreateEntityRequest<TEntity>: BaseEntityRequest, IRequest<Response<TEntity>>, ICreateEntityRequest where TEntity: class, IModel
{

    public string Uid { get; set; } = "";

    public Dictionary<string,object> Delta { get; set; } = new();

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