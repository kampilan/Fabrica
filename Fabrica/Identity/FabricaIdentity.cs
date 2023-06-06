using System.Security.Claims;

namespace Fabrica.Identity;

public class FabricaIdentity: ClaimsIdentity
{


    public FabricaIdentity( IClaimSet claimSet ): base(claimSet.AuthenticationType)
    {

        CheckClaim(FabricaClaims.FlowClaim,       claimSet.AuthenticationFlow);
        CheckClaim(FabricaClaims.TenantClaim,     claimSet.Tenant);
        CheckClaim(FabricaClaims.SubjectClaim,    claimSet.Subject);
        CheckClaim(FabricaClaims.AltSubjectClaim, claimSet.AltSubject);
        CheckClaim(FabricaClaims.UserNameClaim,   claimSet.UserName);
        CheckClaim(FabricaClaims.GivenNameClaim,  claimSet.GivenName);
        CheckClaim(FabricaClaims.FamilyNameClaim, claimSet.FamilyName);
        CheckClaim(FabricaClaims.NameClaim,       claimSet.Name);
        CheckClaim(FabricaClaims.PictureClaim,    claimSet.Picture);
        CheckClaim(FabricaClaims.EmailClaim,      claimSet.Email);

        foreach (var role in claimSet.Roles)
            CheckClaim(FabricaClaims.RoleClaim, role);

        void CheckClaim(string type, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                AddClaim(new Claim(type, value));
        }

    }

}