using MongoDB.Bson;

namespace Fabrica.One.Persistence.Options.Models;

public class ServiceOptions
{


    public ObjectId Id { get; set; }


    public string ServiceName { get; set; } = "";
    public string Environment { get; set; } = "";


    public string TenantId { get; set; } = "";


    public string Description { get; set; } = "";

    public Dictionary<string,object> Configuration { get; set; } = new();


}