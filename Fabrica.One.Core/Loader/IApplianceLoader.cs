using System.Threading.Tasks;
using Fabrica.One.Plan;

namespace Fabrica.One.Loader
{

    public interface IApplianceLoader
    {

        Task Clean( IPlan plan );

        Task Load( IPlan plan, DeploymentUnit unit );

    }

}
