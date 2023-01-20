// ReSharper disable UnusedMember.Global

using Fabrica.Watch;

namespace Fabrica.Identity
{


    public interface IIdentityProvider
    {

        Task<SyncUserResponse> SyncUser( SyncUserRequest request );

    }


    public class SyncUserRequest
    {

        public string IdentityUid { get; set; } = "";

        public string CurrentUsername { get; set; } = "";
        public string CurrentEmail { get; set; } = "";

        public bool? NewEnabled { get; set; }
        public string? NewUsername { get; set; }
        public string? NewEmail { get; set; }
        public string? NewFirstName { get; set; }
        public string? NewLastName { get; set; }

        public Dictionary<string, IEnumerable<string>> Attributes { get; } = new ();

        public List<string> Groups { get; } = new ();

        public bool MustVerifyEmail { get; set; }
        public bool MustUpdateProfile { get; set; }
        public bool MustUpdatePassword { get; set; }
        public bool MustConfigureMfa { get; set; }

        public bool GeneratePassword { get; set; }
        public bool PasswordIsTemporary { get; set; } = true;


    }


    public class SyncUserResponse
    {

        public bool Created { get; set; }
        
        public string IdentityUid { get; set; } = "";

        [Sensitive]
        public string Password { get; set; } = "";

    }


}
