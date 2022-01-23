using AutoMapper;
using Fabrica.Exceptions;
using Fabrica.One.Persistence.Options.Models;
using Fabrica.Utilities.Container;
using MongoDB.Driver;

namespace Fabrica.One.Persistence.Options;

public class ServiceOptionsRepository: CorrelatedObject
{

    public ServiceOptionsRepository( ICorrelation correlation, IMongoDatabase database, IMapper mapper ) : base( correlation )
    {

        Database = database;
        Mapper   = mapper;

    }

    private IMongoDatabase Database { get; }
    private IMapper Mapper { get; }

    private IMongoCollection<ServiceOptions> Services => Database.GetCollection<ServiceOptions>("serviceoptions");

    public async Task<ServiceOptions> GetServiceOptions( string name, string environment, string tenant )
    {

        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        if (string.IsNullOrWhiteSpace(environment)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(environment));
        if (string.IsNullOrWhiteSpace(tenant)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(tenant));

        using var logger = EnterMethod();


        logger.Inspect(nameof(name), name);
        logger.Inspect(nameof(environment), environment);
        logger.Inspect(nameof(tenant), tenant);



        // *****************************************************************
        logger.Debug("Attempting to find Topic by Enviroment and Name");
        var query = await Services.FindAsync(e => e.ServiceName == name && e.Environment == environment && e.TenantId == tenant);
        var service = await query.SingleOrDefaultAsync();

        if (service is null)
            throw new NotFoundException($"Could not find Service Options for {name} in {environment} within {tenant}");

        logger.LogObject(nameof(service), service);



        // *****************************************************************
        return service;


    }


    public async Task Configure( string name, string environment, string tenant, object target )
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to get service");
        var service = await GetServiceOptions( name, environment, tenant );


        // *****************************************************************
        logger.Debug("Attempting to map configuration to supplied target");
        Mapper.Map( service.Configuration, target );

        logger.LogObject(nameof(target), target);


    }


}