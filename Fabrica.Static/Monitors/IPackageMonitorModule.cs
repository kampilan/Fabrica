namespace Fabrica.Static.Monitors
{

    
    public interface IPackageMonitorModule
    {

        string LocalInstallationPath { get; set; }

        string DeploymentName { get; set; }

        int DeploymentMonitorIntervalSecs { get; set; }


    }


}
