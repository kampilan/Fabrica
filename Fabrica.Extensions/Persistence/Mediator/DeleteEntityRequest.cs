using System;
using System.Threading.Tasks;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using MediatR;
using Newtonsoft.Json;


// ReSharper disable UnusedTypeParameter

namespace Fabrica.Persistence.Mediator;


public class DeleteEntityRequest<TEntity>: BaseEntityRequest, IRequest<Response>, IDeleteEntityRequest where TEntity: class, IModel
{
    
    public string Uid { get; set; } = "";

    [JsonIgnore]
    public Func<IMessageMediator, Task<IResponse>> Sender => async (m) => await m.Send(this);

}