

using System.Threading.Tasks;

namespace Fabrica.Utilities.Secrets;

public interface ISecretComponent
{

    Task<string> GetSecret(string key);
    Task<T> GetSecrets<T>() where T: class;

}