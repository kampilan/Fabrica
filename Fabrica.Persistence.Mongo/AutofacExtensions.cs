using Autofac;
using Fabrica.Persistence.Mongo;
using MongoDB.Driver;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Persistence;

public static class AutofacExtensions
{


    public static ContainerBuilder UseMongoDb( this ContainerBuilder builder, string url, string database )
    {

        builder.Register(c =>
            {

                var client = new MongoClient(url);
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