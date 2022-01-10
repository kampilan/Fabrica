
using System.Collections.Generic;
using System.Threading.Tasks;
using Fabrica.One.Models;

namespace Fabrica.One.Repository
{

    public interface IStatusRepository
    {

        Task<IEnumerable<ApplianceModel>> GetAppliances();

    }

}
