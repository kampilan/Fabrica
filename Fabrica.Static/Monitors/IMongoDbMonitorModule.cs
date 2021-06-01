
namespace Fabrica.Static.Monitors
{

    public interface IMongoDbMonitorModule: IPackageMonitorModule
    {

        string MongoDbServerUri { get; set; }
        string MongoDbDatabase  { get; set; }


    }


}
