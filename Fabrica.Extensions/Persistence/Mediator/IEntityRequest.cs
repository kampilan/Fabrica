using System;
using System.Threading.Tasks;
using Fabrica.Mediator;

namespace Fabrica.Persistence.Mediator;

public interface IEntityRequest
{

    public Func<IMessageMediator,Task<IResponse>> Sender { get; }

}