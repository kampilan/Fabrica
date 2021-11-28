using System.Collections.Generic;
using Fabrica.Models.Support;
using Fabrica.Rql;

namespace Fabrica.Persistence.Mediator
{

    public interface IQueryRequest
    {
        bool HasCriteria { get; }
    }
    
    
    public interface IQueryRequest<TModel>: IQueryRequest where TModel: class, IModel
    {

        List<IRqlFilter<TModel>> Filters { get; }

    }
}
