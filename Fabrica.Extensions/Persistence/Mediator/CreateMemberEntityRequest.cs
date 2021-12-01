using System.Collections.Generic;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using MediatR;

// ReSharper disable UnusedTypeParameter

namespace Fabrica.Persistence.Mediator;

public class CreateMemberEntityRequest<TParent,TMember>: ICreateMemberEntityRequest, IRequest<Response<TMember>> where TParent: class, IModel where TMember: class, IModel
{

    public OperationType Operation
    {
        get => OperationType.Create;
        set { }
    }

    public string ParentUid { get; set; } = "";

    public string Uid { get; set; } = "";

    public Dictionary<string,object> Delta { get; set; } = new Dictionary<string,object>();


}