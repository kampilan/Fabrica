namespace Fabrica.Static.Monitors
{


    public interface IFixedPackageMonitorModule: IPackageMonitorModule
    {


        string FixedPackageRespository { get; set; }
        string FixedPackageLocation    { get; set; }


    }


}
