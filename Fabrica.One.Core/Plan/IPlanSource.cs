using System.IO;
using System.Threading.Tasks;

namespace Fabrica.One.Plan
{

    public interface IPlanSource
    {

        Task<Stream> GetSource();

        Task<bool> HasUpdatedPlan();

        Task Reload();

    }


}
