using System;
using System.Linq.Expressions;
using Fabrica.Models.Support;

namespace Fabrica.Persistence.Mediator.Handlers
{

    
    public interface IDuplicateChecked<TResponse> where TResponse: class, IModel
    {

        void AddDuplicateCheck( Func<TResponse, TResponse, string> template, Func<TResponse, Expression<Func<TResponse, bool>>> predicate );

    }



}
