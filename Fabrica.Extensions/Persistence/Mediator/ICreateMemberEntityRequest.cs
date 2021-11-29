namespace Fabrica.Persistence.Mediator;

public  interface ICreateMemberEntityRequest: IDeltaEntityRequest
{


    public string ParentUid { get; }


}