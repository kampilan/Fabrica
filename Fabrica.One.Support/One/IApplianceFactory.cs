using Fabrica.One.Plan;

namespace Fabrica.One
{

    public interface IApplianceFactory
    {

        IAppliance Create( DeploymentUnit unit );

    }

}
