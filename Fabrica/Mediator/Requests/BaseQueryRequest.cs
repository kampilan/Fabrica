using System.Collections.Generic;
using System.Linq;
using Fabrica.Models.Support;
using Fabrica.Rql;

namespace Fabrica.Mediator.Requests
{

    
    public abstract class BaseQueryRequest<TModel>: IQueryRequest<TModel> where TModel: class, IModel
    {

        public List<IRqlFilter<TModel>> Filters { get; set; } = new();

        bool IQueryRequest.HasCriteria => Filters.Any(f => f.HasCriteria);

    }

}
