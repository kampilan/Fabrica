using System.Threading.Tasks;

namespace Fabrica.Utilities.Container;

public interface IRequiresStart
{
    Task Start();

}