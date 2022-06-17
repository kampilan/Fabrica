using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Fabrica.Identity
{


    public static class ClaimsIdentityExtensions
    {


        public static string ToJson(this ClaimsIdentity ci)
        {

            var payload = ci.ToPayload();
            var json = JsonConvert.SerializeObject(payload);

            return json;

        }


        public static void Populate( this ClaimsIdentity ci, string json )
        {
            var payload = JsonConvert.DeserializeObject<ClaimSetModel>(json);
            ci.Populate( payload );

        }


        public static void Populate( this ClaimsIdentity ci, IClaimSet claimSet )
        {

            CheckClaim(FabricaClaims.FlowClaim, claimSet.AuthenticationFlow);
            CheckClaim( FabricaClaims.TenantClaim, claimSet.Tenant );
            CheckClaim( ClaimTypes.NameIdentifier, claimSet.Subject );
            CheckClaim( ClaimTypes.Name, claimSet.Name );
            CheckClaim( FabricaClaims.PictureClaim, claimSet.Picture );
            CheckClaim( ClaimTypes.Email, claimSet.Email );

            foreach (var role in claimSet.Roles)
                CheckClaim(ClaimTypes.Role, role);

            void CheckClaim(string type, string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    ci.AddClaim(new Claim(type, value));
            }

        }


        public static IClaimSet ToPayload(this ClaimsIdentity ci)
        {

            var payload = new ClaimSetModel
            {
                Tenant  = ci.GetTenant(),
                Subject = ci.GetSubject(),
                Name    = ci.GetName(),
                Picture = ci.GetPictureUrl()
            };

            payload.Roles.AddRange( ci.GetRoles() );

            return payload;

        }

        public static string GetTenant( this ClaimsIdentity ci, string missing="" )
        {
            var claim = ci.Claims.FirstOrDefault(c => c.Type == FabricaClaims.TenantClaim);
            return claim?.Value??missing;
        }

        public static string GetSubject( this ClaimsIdentity ci, string missing = "")
        {
            var claim = ci.Claims.FirstOrDefault(c => c.Type == FabricaClaims.SubjectClaim );
            return claim?.Value ?? missing;
        }

        public static string GetName( this ClaimsIdentity ci, string missing = "" )
        {
            var claim = ci.Claims.FirstOrDefault(c => c.Type == FabricaClaims.NameClaim);
            return claim?.Value ?? missing;
        }

        public static string GetEmail( this ClaimsIdentity ci, string missing = "" )
        {
            var claim = ci.Claims.FirstOrDefault(c => c.Type == FabricaClaims.EmailClaim);
            return claim?.Value ?? missing;
        }

        public static string GetPictureUrl( this ClaimsIdentity ci, string missing = "" )
        {
            var claim = ci.Claims.FirstOrDefault(c => c.Type == FabricaClaims.PictureClaim );
            return claim?.Value ?? missing;
        }


        public static IEnumerable<string> GetRoles(this ClaimsIdentity ci)
        {
            var roles = ci.Claims.Where(c => c.Type == ClaimTypes.Role);
            return roles.Select(c => c.Value);
        }


    }


}