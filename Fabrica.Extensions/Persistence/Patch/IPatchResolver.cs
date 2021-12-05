using System.Collections.Generic;
using Fabrica.Models.Patch.Builder;

namespace Fabrica.Persistence.Patch
{

    public interface IPatchResolver
    {

        IEnumerable<PatchRequest> Resolve( PatchSet patchSet );

    }

}
