namespace Fabrica.Persistence.Mediator;

public  interface ICreateMemberEntityRequest: IDeltaEntityRequest, IEntityRequest
{

    public string ParentUid { get; set; }

}