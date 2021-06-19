﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using JetBrains.Annotations;

namespace Fabrica.Mediator.Requests
{

    
    public abstract class BaseQueryRequest<TModel>: IQueryRequest<TModel> where TModel: class, IModel
    {

        public List<IRqlFilter<TModel>> Filters { get; set; } = new();

        bool IQueryRequest.HasCriteria => Filters.Any(f => f.HasCriteria);


        public RqlFilterBuilder<TModel> AddFilter( object criteria=null )
        {
            
            var builder = RqlFilterBuilder<TModel>.Create();
            Filters.Add(builder);

            if (criteria is not null)
                builder.Introspect(criteria);


            return builder;

        }

        public RqlFilterBuilder<TModel> Where<TValue>( [NotNull] Expression<Func<TModel,TValue>> prop )
        {

            if (prop == null) throw new ArgumentNullException(nameof(prop));

            var builder = RqlFilterBuilder<TModel>.Where(prop);
            Filters.Add(builder);

            return builder;

        }

        public RqlFilterBuilder<TModel> FromRql( [NotNull] string rql )
        {

            if (string.IsNullOrWhiteSpace(rql)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(rql));

            var builder = RqlFilterBuilder<TModel>.FromRql( rql );
            Filters.Add(builder);

            return builder;

        }


    }


    public abstract class BaseCriteriaQueryRequest<TModel,TCriteria> : IQueryRequest<TModel> where TModel : class, IModel where TCriteria: class, ICriteria, new()
    {

        public TCriteria Criteria { get; set; } = new ();

        public bool HasCriteria => Criteria is not null;

        public List<IRqlFilter<TModel>> Filters => new() {RqlFilterBuilder<TModel>.Create().Introspect(Criteria)};

    }


}
