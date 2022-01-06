using System.Threading.Tasks;

namespace Fabrica.One.Plan
{


    public interface IPlanWriter
    {

        Task Write( string missionPlanJson );

    }


}
