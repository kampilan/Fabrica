using System.Threading.Tasks;
using Fabrica.Models.Patch.Builder;

namespace Fabrica.Persistence.Patch
{

    public interface IPatchResolverComponent
    {

        Task Apply( PatchSet patchSet, bool checkpoint=true );

    }

}
