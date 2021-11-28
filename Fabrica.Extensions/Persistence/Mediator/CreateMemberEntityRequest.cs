using System.Collections.Generic;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using MediatR;

// ReSharper disable UnusedTypeParameter

namespace Fabrica.Persistence.Mediator;

public class CreateMemberEntityRequest<TParent,TMember>: IRequest<Response<TMember>> where TParent: class, IModel where TMember: class, IModel
{

    public string ParentUid { get; set; } = "";

    public string MemberUid { get; set; } = "";

    public Dictionary<string, object> Properties { get; set; } = new();


}