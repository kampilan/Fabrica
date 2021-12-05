using Fabrica.Exceptions;
using Fabrica.One.Persistence.Work.Models;
using Fabrica.Utilities.Container;
using MongoDB.Driver;

namespace Fabrica.One.Persistence.Work;

public class WorkRepository: CorrelatedObject
{

    public WorkRepository(ICorrelation correlation, IMongoDatabase database) : base(correlation)
    {

        Database = database;

    }


    private IMongoDatabase Database { get; }


    public async Task<WorkTopic> GetTopic(string environment, string name)
    {

        if (string.IsNullOrWhiteSpace(environment)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(environment));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

        using var logger = EnterMethod();

        logger.Inspect(nameof(environment), environment);
        logger.Inspect(nameof(name), name);



        // *****************************************************************
        logger.Debug("Attempting to get Topic collection");
        var collection = Database.GetCollection<WorkTopic>("topics");



        // *****************************************************************
        logger.Debug("Attempting to find Topic by Enviroment and Name");
        var query = await collection.FindAsync(e => e.Environment == environment && e.TopicName == name);
        var topic = await query.SingleOrDefaultAsync();
        if (topic is null)
            throw new NotFoundException($"Could not find Topic using Environment ({environment}) and Name ({name})");

        logger.LogObject(nameof(topic), topic);
        


        // *****************************************************************
        return topic;


    }


}