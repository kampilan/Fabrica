namespace Fabrica.Static.Monitors
{

    public interface IDynamoDbMonitorModule: IPackageMonitorModule
    {

        string TableNamePrefix { get; set; }


    }


}
