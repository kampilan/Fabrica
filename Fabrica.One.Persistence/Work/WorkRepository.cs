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


    private IMongoCollection<WorkTopic> Topics => Database.GetCollection<WorkTopic>("worktopics");


    public async Task<bool> HasTopic( string name )
    {

        using var logger = EnterMethod();

        logger.Inspect(nameof(name), name);



        // *****************************************************************
        logger.Debug("Attempting to find Topic by Enviroment and Name");
        var query  = await Topics.FindAsync(e => e.TopicName == name);
        var exists = await query.AnyAsync();



        // *****************************************************************
        return exists;

    }

    public async Task<WorkTopic> GetTopic( string name )
    {

        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

        using var logger = EnterMethod();

        logger.Inspect(nameof(name), name);



        // *****************************************************************
        logger.Debug("Attempting to find Topic by Enviroment and Name");
        var query = await Topics.FindAsync(e =>e.TopicName == name);
        var topic = await query.SingleOrDefaultAsync();

        logger.LogObject(nameof(topic), topic);
        


        // *****************************************************************
        return topic;


    }


}