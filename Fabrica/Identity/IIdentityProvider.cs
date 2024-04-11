// ReSharper disable UnusedMember.Global

using Fabrica.Watch;

namespace Fabrica.Identity;

public interface IIdentityProvider
{
    Task<bool> ExecutePasswordReset(string email, CancellationToken ct = new());
    Task<SyncUserResponse> SyncUser( SyncUserRequest request, CancellationToken ct=new() );

}


public class SyncUserRequest
{

    public string IdentityUid { get; set; } = string.Empty;

    public string CurrentUsername { get; set; } = string.Empty;
    public string CurrentEmail { get; set; } = string.Empty;

    public bool? NewEnabled { get; set; }
    public string? NewUsername { get; set; }
    public string? NewEmail { get; set; }
    public string? NewFirstName { get; set; }
    public string? NewLastName { get; set; }

    public string? NewPassword { get; set; }

    public Dictionary<string, IEnumerable<string>> Attributes { get; } = new ();

    public List<string> Groups { get; } = new ();

    public bool MustVerifyEmail { get; set; }
    public bool MustUpdateProfile { get; set; }
    public bool MustUpdatePassword { get; set; }
    public bool MustConfigureMfa { get; set; }

    public bool GeneratePassword { get; set; }
    public bool PasswordIsTemporary { get; set; } = true;

    public string HashAlgorithm { get; set; } = string.Empty;
    public int HashIterations { get; set; }
    public string HashedPassword { get; set; } = string.Empty;

    public bool Upsert { get; set; }


}


public class SyncUserResponse
{

    public bool Exists { get; set; }

    public bool Created { get; set; }
    public bool Updated { get; set; }

    public string IdentityUid { get; set; } = string.Empty;
    [Sensitive]
    public string Password { get; set; } = string.Empty;

}



