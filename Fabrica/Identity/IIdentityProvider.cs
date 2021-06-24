using System.Threading.Tasks;
using Fabrica.Watch;

namespace Fabrica.Identity
{


    public interface IIdentityProvider
    {

        Task<SyncUserResponse> SyncUser( string identityUid, string email, string firstName, string lastName );

    }


    public class SyncUserResponse
    {

        public bool Created { get; set; }
        
        public string IdentityUid { get; set; } = "";

        [Sensitive]
        public string Password { get; set; } = "";

    }


}
