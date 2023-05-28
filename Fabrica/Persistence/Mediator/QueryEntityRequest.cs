using System;
using System.Linq.Expressions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using JetBrains.Annotations;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class QueryEntityRequest<TEntity>: BaseEntityRequest, IRequest<Response<List<TEntity>>>, IQueryEntityRequest<TEntity> where TEntity: class, IModel
{


    public static QueryEntityRequest<TEntity> Where( ICriteria criteria, int limit=0 )
    {

        var request = new QueryEntityRequest<TEntity>();
        var builder = request.AddCriteria(criteria);
        builder.RowLimit = limit;

        return request;

    }

    public static QueryEntityRequest<TEntity> Where( IRqlFilter<TEntity> filter )
    {

        var request = new QueryEntityRequest<TEntity>();
        request.Filters.Add(filter);

        return request;

    }


    public List<IRqlFilter<TEntity>> Filters { get; set; } = new();


    bool IQueryEntityRequest.HasCriteria => Filters.Any(f => f.HasCriteria);

    public RqlFilterBuilder<TEntity> AddCriteria( ICriteria criteria = null )
    {

        var builder = RqlFilterBuilder<TEntity>.Create();
        Filters.Add(builder);

        if( criteria is not null )
            builder.Introspect(criteria);


        return builder;

    }

    public void All()
    {

        var builder = RqlFilterBuilder<TEntity>.All();
        Filters.Add(builder);

    }

    public RqlFilterBuilder<TEntity>Where<TValue>( Expression<Func<TEntity,TValue>> predicate )
    {

        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var builder = RqlFilterBuilder<TEntity>.Where(predicate);
        Filters.Add(builder);

        return builder;

    }


}

