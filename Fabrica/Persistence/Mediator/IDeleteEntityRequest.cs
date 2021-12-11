namespace Fabrica.Persistence.Mediator
{

    
    public interface IDeleteEntityRequest: IEntityRequest
    {

        string Uid { get; set; }

    }


}
