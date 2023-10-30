using Autofac;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mongo;
using Fabrica.Persistence.Mongo.Conventions;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Persistence;

public static class AutofacExtensions
{


    public static ContainerBuilder UseMongoDb( this ContainerBuilder builder, string url, string database, bool registerConventions=true )
    {


        if( registerConventions )
        {
            var pack = new ConventionPack
            {
                new ResetClassMapConvention(),
                new PrivateFieldMappingConvention()
            };

            ConventionRegistry.Register("Fabrica.ClassMapping", pack, t => t.IsAssignableTo(typeof(IModel)) && !t.IsAbstract);

        }


        builder.Register(c =>
            {
                var client = new MongoClient(url);
                return client;
            })
            .AsSelf()
            .SingleInstance();


        builder.Register(c =>
            {

                var client = c.Resolve<MongoClient>();
                var db = client.GetDatabase(database);

                var comp = new MongoDbContext(client, db);

                return comp;

            })
            .AsSelf()
            .As<IMongoDbContext>()
            .SingleInstance();

        return builder;

    }

}