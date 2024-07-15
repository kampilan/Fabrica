using Fabrica.Rql.Builder;

namespace Fabrica.Rql.Parser;

public class RqlParserComponentImpl : IRqlParserComponent
{

    public RqlFilterBuilder Parse(string rql)
    {


        // *****************************************************************
        if (string.IsNullOrWhiteSpace(rql))
        {
            var builder = RqlFilterBuilder.Create();

            return builder;
        }
        else
        {

            var tree = RqlLanguageParser.ToFilter(rql);

            var builder = new RqlFilterBuilder(tree);

            return builder;
        }


    }


    public RqlFilterBuilder ParseCriteria(string rql)
    {


        // *****************************************************************
        if (string.IsNullOrWhiteSpace(rql))
        {
            var builder = RqlFilterBuilder.Create();

            return builder;
        }
        else
        {

            var tree = RqlLanguageParser.ToCriteria(rql);

            var builder = new RqlFilterBuilder(tree);

            return builder;
        }


    }


    public RqlFilterBuilder<TEntity> Parse<TEntity>(string rql) where TEntity : class
    {

        // *****************************************************************
        if (string.IsNullOrWhiteSpace(rql))
        {
            var builder = RqlFilterBuilder<TEntity>.Create();

            return builder;
        }
        else
        {

            var tree = RqlLanguageParser.ToFilter(rql);

            var builder = new RqlFilterBuilder<TEntity>(tree);

            return builder;
        }


    }


    public RqlFilterBuilder<TEntity> ParseCriteria<TEntity>(string rql) where TEntity : class
    {


        // *****************************************************************
        if (string.IsNullOrWhiteSpace(rql))
        {
            var builder = RqlFilterBuilder<TEntity>.Create();

            return builder;
        }
        else
        {

            var tree = RqlLanguageParser.ToCriteria(rql);

            var builder = new RqlFilterBuilder<TEntity>(tree);

            return builder;
        }


    }



}