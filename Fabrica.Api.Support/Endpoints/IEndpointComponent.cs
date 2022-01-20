
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Patch;
using Fabrica.Utilities.Container;

namespace Fabrica.Api.Support.Endpoints;

public interface IEndpointComponent
{

    public ICorrelation Correlation { get; }
    public IModelMetaService Meta { get; }
    public IMessageMediator Mediator { get; }
    public IPatchResolver Resolver { get; }

}