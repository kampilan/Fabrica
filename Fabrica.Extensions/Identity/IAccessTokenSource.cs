using System.Threading.Tasks;

namespace Fabrica.Identity;

public interface IAccessTokenSource
{

    bool HasExpired { get; }
    Task<string> GetToken();

}