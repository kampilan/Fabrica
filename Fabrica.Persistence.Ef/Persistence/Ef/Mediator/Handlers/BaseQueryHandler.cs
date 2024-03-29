﻿using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Persistence.Mediator;
using Fabrica.Rql;
using Fabrica.Rql.Serialization;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using MediatR;
using Microsoft.EntityFrameworkCore;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Persistence.Ef.Mediator.Handlers;

public abstract class BaseQueryHandler<TRequest, TResponse, TDbContext> : BaseHandler<TRequest, List<TResponse>> where TRequest : class, IRequest<Response<List<TResponse>>>, IQueryEntityRequest<TResponse> where TResponse: class, IModel where TDbContext: DbContext, IReplicaDbContext
{


    protected BaseQueryHandler(ICorrelation correlation, IRuleSet rules, TDbContext context) : base(correlation)
    {
        Rules = rules;
        Context = context;
    }


    protected IRuleSet Rules { get; }
    protected TDbContext Context { get; }


    protected abstract Func<TDbContext,IQueryable<TResponse>> Many { get; }


    protected async Task<List<TResponse>> ProcessFilters( IEnumerable<IRqlFilter<TResponse>> filters, CancellationToken token )
    {

        if (filters == null) throw new ArgumentNullException(nameof(filters));

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to evaluate filters");
        var filterList = filters.ToList();

        var ec = Rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;

        ec.AddAllFacts(filterList);

        var er = Rules.Evaluate(ec);

        logger.LogObject(nameof(er), er);



        // *****************************************************************
        logger.Debug("Attempting to process each given filter");
        var set = new HashSet<TResponse>();
        foreach( var filter in filterList )
        {

            var queryable = Many(Context).Where(filter.ToExpression());

            List<TResponse> result;
            if( filter.RowLimit > 0 )
                result = await queryable.AsNoTracking().Take(filter.RowLimit).ToListAsync(cancellationToken: token);
            else
                result = await queryable.AsNoTracking().ToListAsync(cancellationToken: token);

            set.UnionWith(result);

        }



        // *****************************************************************
        logger.Debug("Attempting to create list from union set");
        var list = set.ToList();

        logger.Inspect(nameof(list.Count), list.Count);



        // *****************************************************************
        return list;


    }


    protected override async Task<List<TResponse>> Perform( CancellationToken cancellationToken = default )
    {

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to process filters");
        var result = await ProcessFilters(Request.Filters, cancellationToken);

        logger.Inspect(nameof(result.Count), result.Count);



        // *****************************************************************
        return result;


    }


}