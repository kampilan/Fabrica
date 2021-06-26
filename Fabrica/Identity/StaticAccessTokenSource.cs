using System.Threading.Tasks;

namespace Fabrica.Identity
{

    public class StaticAccessTokenSource: IAccessTokenSource
    {


        public StaticAccessTokenSource(string token)
        {
            AccessToken = token;
        }

        private string AccessToken { get; }

        public bool HasExpired => false;
        public Task<string> GetToken()
        {
            return Task.FromResult(AccessToken);
        }


    }


}
