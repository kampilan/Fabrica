using System.Threading.Tasks;
using Fabrica.Watch;
using JetBrains.Annotations;

namespace Fabrica.Identity
{


    public interface IIdentityProvider
    {

        Task<SyncUserResponse> SyncUser( [NotNull] SyncUserRequest request );

    }


    public class SyncUserRequest
    {

        public string IdentityUid { get; set; } = "";
        public string CurrentEmail { get; set; } = "";

        public string NewEmail { get; set; }
        public string NewFirstName { get; set; }
        public string NewLastName { get; set; }

    }


    public class SyncUserResponse
    {

        public bool Created { get; set; }
        
        public string IdentityUid { get; set; } = "";

        [Sensitive]
        public string Password { get; set; } = "";

    }


}
