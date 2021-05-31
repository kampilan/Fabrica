using System.Security.Claims;

namespace Fabrica.Identity
{


    public class FabricaIdentity: ClaimsIdentity
    {


        public FabricaIdentity( IClaimSet claimSet ): base(claimSet.AuthenticationType)
        {

            CheckClaim(FabricaClaims.TenantClaim, claimSet.Tenant);
            CheckClaim(ClaimTypes.NameIdentifier, claimSet.Subject);
            CheckClaim(ClaimTypes.Name, claimSet.Name);
            CheckClaim(FabricaClaims.PictureClaim, claimSet.Picture);
            CheckClaim(ClaimTypes.Email, claimSet.Email);

            foreach (var role in claimSet.Roles)
                CheckClaim(ClaimTypes.Role, role);

            void CheckClaim(string type, string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    AddClaim(new Claim(type, value));
            }

        }

    }


}
