using Fabrica.Rql.Parser;
using Fabrica.Rql;
using Fabrica.Rql.Builder;
using MongoDB.Driver;

namespace Fabrica.Persistence.Mongo.Rql;

public static class MongoRqlSerializer
{


    public static FilterDefinition<TEntity> BuildFilter<TEntity>( ICriteria criteria ) where TEntity : class
    {

        var builder = RqlFilterBuilder<TEntity>
            .Create()
            .Introspect(criteria);

        var filter = BuildFilter(builder);

        return filter;

    }


    public static FilterDefinition<TEntity> BuildFilter<TEntity>( string rql ) where TEntity : class
    {

        var tree = RqlLanguageParser.ToCriteria(rql);
        var builder = new RqlFilterBuilder<TEntity>(tree);

        var filter = BuildFilter(builder);

        return filter;

    }


    public static FilterDefinition<TEntity> BuildFilter<TEntity>( IRqlFilter<TEntity> builder ) where TEntity : class
    {

        var definition = new FilterDefinitionBuilder<TEntity>();

        if( !builder.HasCriteria )
            return definition.Empty;

        var filter = FilterDefinition<TEntity>.Empty;


        foreach (var op in builder.Criteria)
        {

            switch (op.Operator)
            {
                case RqlOperator.NotSet:
                    break;
                case RqlOperator.Equals:
                    filter &= definition.Eq(op.Target.Name, op.Values[0]);
                    break;
                case RqlOperator.NotEquals:
                    filter &= definition.Ne(op.Target.Name, op.Values[0]);
                    break;
                case RqlOperator.LesserThan:
                    filter &= definition.Lt(op.Target.Name, op.Values[0]);
                    break;
                case RqlOperator.GreaterThan:
                    filter &= definition.Gt(op.Target.Name, op.Values[0]);
                    break;
                case RqlOperator.LesserThanOrEqual:
                    filter &= definition.Lte(op.Target.Name, op.Values[0]);
                    break;
                case RqlOperator.GreaterThanOrEqual:
                    filter &= definition.Gte(op.Target.Name, op.Values[0]);
                    break;
                case RqlOperator.StartsWith:
                    filter &= definition.Regex(op.Target.Name, $"^{op.Values[0]}");
                    break;
                case RqlOperator.Contains:
                    filter &= definition.Regex(op.Target.Name, $"^.*{op.Values[0]}.*$");
                    break;
                case RqlOperator.Between:
                    filter &= definition.Gte(op.Target.Name, op.Values[0]);
                    filter &= definition.Lte(op.Target.Name, op.Values[1]);
                    break;
                case RqlOperator.In:
                    filter &= definition.In(op.Target.Name, op.Values);
                    break;
                case RqlOperator.NotIn:
                    filter &= definition.Nin(op.Target.Name, op.Values);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


        }


        return filter;


    }

}