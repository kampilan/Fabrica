using Fabrica.Models;
using Fabrica.Models.Support;
using Fabrica.Rql.Builder;

namespace Fabrica.Rql
{


    public interface IRqlParserComponent
    {

        RqlFilterBuilder Parse(string rql);

        RqlFilterBuilder ParseCriteria(string rql);

        RqlFilterBuilder<TEntity> Parse<TEntity>( string rql ) where TEntity : class, IModel;

        RqlFilterBuilder<TEntity> ParseCriteria<TEntity>(string rql) where TEntity : class, IModel;

    }


}
