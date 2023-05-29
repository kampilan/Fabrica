using System.Reflection;
using Fabrica.Models.Support;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Humanizer;
using MongoDB.Driver;

namespace Fabrica.Persistence.Mongo;


public interface IMongoDbContext
{
    IMongoCollection<TEntity> GetCollection<TEntity>( string name="" );

}

public class MongoDbContext: IMongoDbContext
{


    public MongoDbContext( MongoClient client, IMongoDatabase database )
    {

        Client   = client;
        Database = database;
    }

    private MongoClient Client { get; }
    private IMongoDatabase Database { get; }


    public IMongoCollection<TEntity> GetCollection<TEntity>( string name="" )
    {

        using var logger = this.EnterMethod();

        logger.Inspect(nameof(name), name);

        if( string.IsNullOrWhiteSpace(name) )
        {

            var attr = typeof(TEntity).GetCustomAttribute<CollectionAttribute>();
            if (attr is not null && !string.IsNullOrWhiteSpace(attr.Name))
                name = attr.Name.Pluralize().ToLowerInvariant();
            else
                name = typeof(TEntity).Name.Pluralize().ToLowerInvariant();

        }

        logger.Inspect(nameof(name), name);



        // *****************************************************************
        logger.Debug("Attempting to get MongoDB Collection from Database");
        var collection = Database.GetCollection<TEntity>(name);



        // *****************************************************************
        return collection;

    }

}