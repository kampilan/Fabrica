using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Parser;
using JetBrains.Annotations;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class QueryEntityRequest<TEntity>: IQueryEntityRequest<TEntity>, IRequest<Response<List<TEntity>>> where TEntity: class, IModel
{

    public List<IRqlFilter<TEntity>> Filters { get; set; } = new();

    bool IQueryEntityRequest.HasCriteria => Filters.Any(f => f.HasCriteria);

    public RqlFilterBuilder<TEntity> AddFilter( ICriteria criteria = null )
    {

        var builder = RqlFilterBuilder<TEntity>.Create();
        Filters.Add(builder);

        if (criteria is not null)
            builder.Introspect(criteria);


        return builder;

    }

    public RqlFilterBuilder<TEntity> Where<TValue>([NotNull] Expression<Func<TEntity, TValue>> predicate)
    {

        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var builder = RqlFilterBuilder<TEntity>.Where(predicate);
        Filters.Add(builder);

        return builder;

    }

    public RqlFilterBuilder<TEntity> FromRql([NotNull] string rql)
    {

        if (string.IsNullOrWhiteSpace(rql)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(rql));

        var tree = RqlLanguageParser.ToCriteria(rql);

        var builder = new RqlFilterBuilder<TEntity>(tree);
        Filters.Add(builder);

        return builder;

    }


}


public class QueryEntityRequest<TModel,TCriteria> : IQueryEntityRequest<TModel>, IRequest<Response<List<TModel>>> where TModel: class, IModel where TCriteria : class, ICriteria, new()
{

    public TCriteria Criteria { get; set; } = new();

    public bool HasCriteria => Criteria is not null;


    public List<IRqlFilter<TModel>> Filters
    {

        get
        {

            var filters = new List<IRqlFilter<TModel>>();

            if (Criteria is not null && Criteria.Rql is not null && Criteria.Rql.Length > 0)
                filters.AddRange(Criteria.Rql.Select(s =>
                {
                    var tree = RqlLanguageParser.ToCriteria(s);
                    return new RqlFilterBuilder<TModel>(tree);
                }));
            else if (Criteria is not null)
                filters.Add(RqlFilterBuilder<TModel>.Create().Introspect(Criteria));

            return filters;

        }

    }

}