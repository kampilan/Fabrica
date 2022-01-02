using System.Threading.Tasks;
using Fabrica.One.Plan;

namespace Fabrica.One.Installer
{

    public interface IApplianceInstaller
    {

        Task Clean( IPlan plan );

        Task Install( IPlan plan, DeploymentUnit unit );

    }

}
