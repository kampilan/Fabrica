using Fabrica.One.Plan;
using JetBrains.Annotations;

namespace Fabrica.One
{

    public interface IApplianceFactory
    {

        IAppliance Create( [NotNull] IPlan plan, [NotNull] DeploymentUnit unit );

    }

}
