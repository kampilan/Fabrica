using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mediator;
using Fabrica.Rql.Serialization;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using MediatR;
using MongoDB.Driver;

namespace Fabrica.Persistence.Mongo.Mediator.Handlers;

// ReSharper disable once UnusedMember.Global

public abstract class BaseQueryHandler<TRequest, TResponse> : BaseHandler<TRequest, List<TResponse>> where TRequest : class, IRequest<Response<List<TResponse>>>, IQueryEntityRequest<TResponse> where TResponse : class, IModel
{


    protected BaseQueryHandler(ICorrelation correlation, IRuleSet rules, IMongoDbContext context) : base(correlation)
    {

        Rules = rules;
        Collection = context.GetCollection<TResponse>();

    }


    protected IRuleSet Rules { get; }
    protected IMongoCollection<TResponse> Collection { get; }


    protected override async Task<List<TResponse>> Perform(CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to evaluate filters");
        var filterList = Request.Filters.ToList();

        var ec = Rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;

        ec.AddAllFacts(filterList);

        var er = Rules.Evaluate(ec);

        logger.LogObject(nameof(er), er);



        // *****************************************************************
        logger.Debug("Attempting to process each given filter");
        var set = new HashSet<TResponse>();
        foreach (var filter in filterList)
        {

            IFindFluent<TResponse, TResponse> cursor;
            if (filter.RowLimit > 0)
                cursor = Collection.Find(filter.ToExpression()).Limit(filter.RowLimit);
            else
                cursor = Collection.Find(filter.ToExpression());

            var list = await cursor.ToListAsync(cancellationToken);

            set.UnionWith(list);

        }



        // *****************************************************************
        logger.Debug("Attempting to create list from union set");
        var result = set.ToList();

        logger.Inspect(nameof(result.Count), result.Count);




        // *****************************************************************
        return result;


    }


}