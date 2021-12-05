using System;
using System.Threading.Tasks;
using Fabrica.Mediator;
using Fabrica.Models.Patch.Builder;

namespace Fabrica.Persistence.Patch;

public class PatchRequest
{


    public PatchRequest( ModelPatch source, Func<IMessageMediator,Task<IResponse>> action )
    {
        Source = source;
        Action = action;
    }    


    public ModelPatch Source { get; set; }

    private Func<IMessageMediator, Task<IResponse>> Action { get; }
    public async Task<IResponse> Apply( IMessageMediator mediator )
    {
        var response = await Action(mediator);
        return response;
    }

}