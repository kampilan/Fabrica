using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Amazon.S3;
using Autofac;
using Fabrica.Static.Providers.Mutable;
using Fabrica.Watch;

namespace Fabrica.Static.Monitors
{

    
    public class FixedPackageMonitor: AbstractPackageMonitor, IStartable, IDisposable
    {



        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class DeploymentInfo : IDeploymentInfo
        {

            public string Name { get; set; } = "";
            
            public string PackageRepository { get; set; } = "";
            public string PackageLocation   { get; set; } = "";

            public DateTime BuildDate { get; set; } = DateTime.Now;

        }



        public FixedPackageMonitor( IFixedPackageMonitorModule config,  IAmazonS3 client, MutableDirectoryFileProvider provider ): base( config, client, provider )
        {

            FixedPackageRepository = config.FixedPackageRespository;
            FixedPackageLocation   = config.FixedPackageLocation;

        }

        public string FixedPackageRepository { get; set; }
        public string FixedPackageLocation { get; set; }



        protected override Task<IDeploymentInfo> GetDeploymentInfo()
        {

            var info = new DeploymentInfo
            {
                PackageRepository = FixedPackageRepository,
                PackageLocation   = FixedPackageLocation,
                BuildDate         = DateTime.Now
            };


            return Task.FromResult((IDeploymentInfo)info);

        }


    }




}
