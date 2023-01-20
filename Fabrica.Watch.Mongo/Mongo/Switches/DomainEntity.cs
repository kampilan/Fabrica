
// ReSharper disable UnusedMember.Global

using MongoDB.Bson;

namespace Fabrica.Watch.Mongo.Switches
{


    public class DomainEntity
    {

        public ObjectId Id { get; set; }

        public string? Uid { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }

        public string? ServerUri { get; set; }
        public string? Database { get; set; }
        public string? Collection { get; set; }


    }


}
