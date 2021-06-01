using System;

namespace Fabrica.Static.Monitors
{


    public interface IDeploymentInfo
    {

        string Name { get; }
        
        string PackageRepository { get; }
        string PackageLocation   { get; }

        DateTime BuildDate { get; }

    }


}
