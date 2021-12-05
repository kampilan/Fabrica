using System.Collections.Generic;
using Fabrica.Models.Support;
using Fabrica.Rql;
using MediatR;

namespace Fabrica.Persistence.Mediator
{

    public interface IQueryEntityRequest
    {
        bool HasCriteria { get; }
    }
    
    
    public interface IQueryEntityRequest<TModel>: IQueryEntityRequest where TModel: class, IModel
    {

        List<IRqlFilter<TModel>> Filters { get; }

    }
}
