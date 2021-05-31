using System.Threading.Tasks;
using Fabrica.One.Plan;

namespace Fabrica.One.Loader
{

    public interface IApplianceLoader
    {

        Task Load( DeploymentUnit unit );

    }

}
