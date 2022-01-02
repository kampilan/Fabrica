using System.Collections.Generic;

namespace Fabrica.One.Plan
{


    public interface IPlan
    {

        string Name { get;  }

        string RepositoryVersion { get; }

        bool DeployAppliances { get;  }
        bool StartAppliances { get;  }
        bool StartInParallel { get;  }
        bool AllAppliancesMustDeploy { get; }


        int WaitForDeploySeconds { get;  }
        int WaitForStartSeconds { get;  }
        int WaitForStopSeconds { get;  }

        List<DeploymentUnit> Deployments { get;  }


        string RepositoryRoot { get; }

        string InstallationRoot { get; }


    }


}