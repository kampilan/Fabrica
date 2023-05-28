using Humanizer;
using MongoDB.Driver;

namespace Fabrica.Persistence.Mongo;


public interface IMongoDbContext
{
    IMongoCollection<TEntity> GetCollection<TEntity>( string name="" );

}

public class MongoDbContext: IMongoDbContext
{


    public MongoDbContext(MongoClient client, IMongoDatabase database)
    {

        Client   = client;
        Database = database;
    }

    private MongoClient Client { get; }
    private IMongoDatabase Database { get; }


    public IMongoCollection<TEntity> GetCollection<TEntity>( string name="" )
    {

        if( string.IsNullOrWhiteSpace(name))
            name = typeof(TEntity).Name.Pluralize();

        var collection = Database.GetCollection<TEntity>(name);

        return collection;

    }

}