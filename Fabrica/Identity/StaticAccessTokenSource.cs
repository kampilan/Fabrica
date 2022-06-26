using System.Threading.Tasks;

namespace Fabrica.Identity
{

    public class StaticAccessTokenSource: IAccessTokenSource
    {

        public StaticAccessTokenSource( string name, string token)
        {
            Name        = name;
            AccessToken = token;
        }

        private string AccessToken { get; }

        public string Name { get; }

        public bool HasExpired => false;
        public Task<string> GetToken()
        {
            return Task.FromResult(AccessToken);
        }


    }


}
