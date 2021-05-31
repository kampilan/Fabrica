using System.Security.Claims;

namespace Fabrica.Identity
{


    public static class FabricaClaims
    {
        
        public static string Scheme => "Fabrica.ProxyToken";
        public static string Policy => "Fabrica.ProxyToken";

        public static string TenantClaim  = "https://fabrica.com/claims/tenant";
        public static string SubjectClaim = ClaimTypes.NameIdentifier;
        public static string NameClaim    = ClaimTypes.Name;
        public static string EmailClaim   = ClaimTypes.Email;

        public static string PictureClaim = "picture";


    }


}