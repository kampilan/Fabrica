using MongoDB.Bson;

namespace Fabrica.One.Persistence.Work.Models;

public class WorkTopic
{

    public ObjectId Id { get; set; }

    public string Environment { get; set; } = "";
    public string TopicName { get; set; } = "";

    public string Description { get; set; } = "";

    public string Endpoint { get; set; } = "";


}