using Fabrica.One.Plan;

namespace Fabrica.One
{


    public class ApplianceFactory: IApplianceFactory
    {

        public IAppliance Create( DeploymentUnit unit )
        {
            return new Appliance( unit );
        }

    }


}
