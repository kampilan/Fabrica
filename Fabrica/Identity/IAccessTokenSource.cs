using System.Threading.Tasks;

namespace Fabrica.Identity;

public interface IAccessTokenSource
{

    string Name { get; }
    bool HasExpired { get; }
    Task<string> GetToken();

}