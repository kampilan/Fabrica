using System.Collections.Generic;

namespace Fabrica.One.Plan
{


    public interface IPlan
    {

        string Name { get; set; }
        string Environment { get; set; }



        bool RealtimeLogging { get; }
        string WatchEventStoreUri { get; }
        string WatchDomainName { get; }


        string RepositoryRoot { get; set; }
        string InstallationRoot { get; set; }
        string ConfigurationType { get; set; }


        bool DeployAppliances { get; set; }
        bool StartAppliances { get; set; }
        bool StartInParallel { get; set; }
        bool AllAppliancesMustDeploy { get; set; }


        int WaitForDeploySeconds { get; set; }
        int WaitForStartSeconds { get; set; }
        int WaitForStopSeconds { get; set; }


        Dictionary<string, object> Configuration { get; set; }


        void Validate();


        List<DeploymentUnit> Deployments { get; set; }

    }


}