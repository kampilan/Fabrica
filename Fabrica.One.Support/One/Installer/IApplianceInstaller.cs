using System.Threading.Tasks;
using Fabrica.One.Plan;

namespace Fabrica.One.Installer
{

    public interface IApplianceInstaller
    {

        void Clean( IPlan plan );

        Task Install( DeploymentUnit unit );

    }

}
