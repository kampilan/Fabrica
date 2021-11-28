using Fabrica.Mediator.Requests;

namespace Fabrica.Persistence.Mediator;

public  interface ICreateMemberRequest: ICreateRequest
{

    public string ParentUid { get; }

}