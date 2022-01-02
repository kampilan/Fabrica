using System;
using Fabrica.One.Plan;

namespace Fabrica.One
{


    public class ApplianceFactory: IApplianceFactory
    {

        public IAppliance Create( IPlan plan, DeploymentUnit unit )
        {

            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (unit == null) throw new ArgumentNullException(nameof(unit));

            return new Appliance( plan, unit );

        }

    }


}
