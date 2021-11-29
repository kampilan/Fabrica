using Fabrica.Mediator;
using Fabrica.Models.Support;
using MediatR;

// ReSharper disable UnusedTypeParameter

namespace Fabrica.Persistence.Mediator;

public class CreateMemberEntityRequest<TParent,TMember,TDelta>: ICreateMemberEntityRequest, IRequest<Response<TMember>> where TParent: class, IModel where TMember: class, IModel where TDelta: BaseDelta, new()
{

    public OperationType Operation
    {
        get => OperationType.Create;
        set { }
    }

    public string ParentUid { get; set; } = "";

    public string Uid { get; set; } = "";

    public TDelta Delta { get; set; } = new();

    BaseDelta IDeltaEntityRequest.Delta => Delta;


}