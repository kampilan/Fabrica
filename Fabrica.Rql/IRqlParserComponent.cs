using Fabrica.Rql.Builder;

namespace Fabrica.Rql;

public interface IRqlParserComponent
{

    RqlFilterBuilder Parse(string rql);

    RqlFilterBuilder ParseCriteria(string rql);

    RqlFilterBuilder<TEntity> Parse<TEntity>( string rql ) where TEntity : class;

    RqlFilterBuilder<TEntity> ParseCriteria<TEntity>(string rql) where TEntity : class;

}