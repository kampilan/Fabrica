using System.Reflection;
using System.Text.Json.Serialization;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using JetBrains.Annotations;
using MediatR;

// ReSharper disable UnusedTypeParameter

namespace Fabrica.Persistence.Mediator;

public class CreateMemberEntityRequest<TParent,TMember>: BaseEntityRequest, IRequest<Response<TMember>>, ICreateMemberEntityRequest where TParent: class, IModel where TMember: class, IModel
{

    public string ParentUid { get; set; } = "";

    public string Uid { get; set; } = "";

    public Dictionary<string,object> Delta { get; set; } = new ();


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