using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Fabrica.Rql.Serialization;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Mediator.Handlers
{


    public abstract class BaseQueryHandler<TRequest, TResponse> : BaseHandler<TRequest, TResponse> where TRequest : class, IRequest<Response<TResponse>>
    {


        protected BaseQueryHandler(ICorrelation correlation, IRuleSet rules, DbContext context) : base(correlation)
        {
            Rules = rules;
            Context = context;
        }


        protected IRuleSet Rules { get; }
        protected DbContext Context { get; }


        protected async Task<List<TModel>> ProcessFilters<TModel>([NotNull] IEnumerable<IRqlFilter<TModel>> filters, CancellationToken token) where TModel : class, IModel
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
            var set = new HashSet<TModel>();
            foreach (var queryable in filterList.Select(filter => filter.ToExpression()).Select(predicate => Context.Set<TModel>().Where(predicate)))
            {
                var result = await queryable.ToListAsync(cancellationToken: token);
                set.UnionWith(result);
            }



            // *****************************************************************
            logger.Debug("Attempting to create list from union set");
            var list = set.ToList();

            logger.Inspect(nameof(list.Count), list.Count);



            // *****************************************************************
            return list;


        }



    }

}
