﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using JetBrains.Annotations;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class QueryThinEntityRequest<TEntity> : BaseEntityRequest, IRequest<Response<MemoryStream>>, IQueryEntityRequest<TEntity> where TEntity : class, IModel
{


    public static QueryThinEntityRequest<TEntity> Where([NotNull] ICriteria criteria, int limit = 0)
    {

        var request = new QueryThinEntityRequest<TEntity>();
        var builder = request.AddCriteria(criteria);
        builder.RowLimit = limit;

        return request;

    }

    public static QueryThinEntityRequest<TEntity> Where([NotNull] IRqlFilter<TEntity> filter)
    {

        var request = new QueryThinEntityRequest<TEntity>();
        request.Filters.Add(filter);

        return request;

    }


    bool IQueryEntityRequest.HasCriteria => Filters.Any(f => f.HasCriteria);

    public List<IRqlFilter<TEntity>> Filters { get; set; } = new();


    public RqlFilterBuilder<TEntity> AddCriteria(ICriteria criteria = null)
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


}
