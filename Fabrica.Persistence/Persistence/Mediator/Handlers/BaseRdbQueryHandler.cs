using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Mediator;
using Fabrica.Mediator.Requests;
using Fabrica.Models.Support;
using Fabrica.Rql;
using Fabrica.Rql.Parser;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using MediatR;
using RepoDb;
using RepoDb.Enumerations;

namespace Fabrica.Persistence.Mediator.Handlers
{


    public abstract class BaseRdbQueryHandler<TRequest, TResponse> : BaseHandler<TRequest, List<TResponse>> where TRequest : class, IRequest<Response<List<TResponse>>>, IQueryRequest<TResponse> where TResponse : class, IModel
    {


        protected BaseRdbQueryHandler( IDbConnection connection, IRuleSet rules,  ICorrelation correlation ) : base(correlation)
        {

            Connection = connection;
            Rules      = rules;

        }

        private IDbConnection Connection { get; }
        private IRuleSet Rules { get; }


        protected override async Task<List<TResponse>> Perform(CancellationToken cancellationToken = default)
        {

            using var logger = EnterMethod();


            using( Connection )
            {


                // *****************************************************************
                logger.Debug("Attempting to ensure connection is open");
                await Connection.EnsureOpenAsync(cancellationToken: cancellationToken);



                // *****************************************************************
                logger.Debug("Attempting to evaluate filters");
                var filterList = Request.Filters;

                var ec = Rules.GetEvaluationContext();
                ec.ThrowNoRulesException = false;

                ec.AddAllFacts(filterList);

                var er = Rules.Evaluate(ec);

                logger.LogObject(nameof(er), er);



                // *****************************************************************
                logger.Debug("Attempting to process each given filter");
                var set = new HashSet<TResponse>();
                foreach( var predicate in filterList.Select(filter => filter.ToRdbQueryFields()))
                {
                    var result = await Connection.QueryAsync<TResponse>( predicate, cancellationToken: cancellationToken);
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


    public static class RepoDbExtensions
    {


        public static List<QueryField> ToRdbQueryFields<TModel>( this IRqlFilter<TModel> filter ) where TModel : class, IModel
        {

            var fields = new List<QueryField>();

            foreach (var pred in filter.Criteria)
            {

                switch (pred.Operator)
                {
                    case RqlOperator.Equals:
                        fields.Add( new QueryField(pred.Target.Name,Operation.Equal, pred.Values[0]));
                        break;
                    case RqlOperator.NotEquals:
                        fields.Add(new QueryField(pred.Target.Name, Operation.NotEqual, pred.Values[0]));
                        break;
                    case RqlOperator.LesserThan:
                        fields.Add(new QueryField(pred.Target.Name, Operation.LessThan, pred.Values[0]));
                        break;
                    case RqlOperator.GreaterThan:
                        fields.Add(new QueryField(pred.Target.Name, Operation.GreaterThan, pred.Values[0]));
                        break;
                    case RqlOperator.LesserThanOrEqual:
                        fields.Add(new QueryField(pred.Target.Name, Operation.LessThanOrEqual, pred.Values[0]));
                        break;
                    case RqlOperator.GreaterThanOrEqual:
                        fields.Add(new QueryField(pred.Target.Name, Operation.GreaterThanOrEqual, pred.Values[0]));
                        break;
                    case RqlOperator.StartsWith:
                        fields.Add(new QueryField(pred.Target.Name, Operation.Like, $"{pred.Values[0]}%"));
                        break;
                    case RqlOperator.Contains:
                        fields.Add(new QueryField(pred.Target.Name, Operation.Like, $"%{pred.Values[0]}%"));
                        break;
                    case RqlOperator.Between:
                        fields.Add(new QueryField(pred.Target.Name, Operation.Between, pred.Values ) );
                        break;
                    case RqlOperator.In:
                        fields.Add(new QueryField(pred.Target.Name, Operation.In, pred.Values));
                        break;
                    case RqlOperator.NotIn:
                        fields.Add(new QueryField(pred.Target.Name, Operation.NotIn, pred.Values));
                        break;
                    case RqlOperator.All:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


            }

            return fields;

        }


    }


}
