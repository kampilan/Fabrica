using System.Linq;
using Fabrica.Mediator;
using Fabrica.Models.Patch.Builder;
using Fabrica.Models.Support;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class PatchEntityRequest<TEntity>: IRequest<Response<TEntity>> where TEntity: class, IMutableModel
{

    public string Uid { get; set; } = "";

    public PatchSet Patches { get; } = new ();

    public void FromModel( params TEntity[] sources )
    {
        
        Patches.Add( sources.Cast<IMutableModel>() );

    }        


}