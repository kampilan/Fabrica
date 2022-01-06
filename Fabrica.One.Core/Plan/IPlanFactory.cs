using System.Threading.Tasks;

namespace Fabrica.One.Plan
{


    public interface IPlanFactory
    {

        Task<IPlan> Create( IPlanSource source, bool produceEmptyPlan=false );

        Task CreateRepositoryVersion( IPlan plan );

        Task Save( IPlan plan, IPlanWriter writer );

    }


}
