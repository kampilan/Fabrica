using System.Security.Claims;

namespace Fabrica.Identity;

public static class ClaimsPrincipleExtensions
{


    public static string GetAuthType(this ClaimsPrincipal cp, string missing = "")
    {
        var claim = cp.Claims.FirstOrDefault(c => c.Type == FabricaClaims.TypeClaim);
        return claim?.Value ?? missing;
    }


    public static string GetAuthFlow(this ClaimsPrincipal cp, string missing = "")
    {
        var claim = cp.Claims.FirstOrDefault(c => c.Type == FabricaClaims.FlowClaim);
        return claim?.Value ?? missing;
    }


    public static string GetTenant(this ClaimsPrincipal cp, string missing = "")
    {
        var claim = cp.Claims.FirstOrDefault(c => c.Type == FabricaClaims.TenantClaim);
        return claim?.Value ?? missing;
    }

    public static string GetSubject(this ClaimsPrincipal cp, string missing = "")
    {
        var claim = cp.Claims.FirstOrDefault(c => c.Type == FabricaClaims.SubjectClaim);
        return claim?.Value ?? missing;
    }

    public static string GetAltSubject(this ClaimsPrincipal cp, string missing = "")
    {
        var claim = cp.Claims.FirstOrDefault(c => c.Type == FabricaClaims.AltSubjectClaim);
        return claim?.Value ?? missing;
    }

    public static string GetUserName(this ClaimsPrincipal cp, string missing = "")
    {
        var claim = cp.Claims.FirstOrDefault(c => c.Type == FabricaClaims.UserNameClaim);
        return claim?.Value ?? missing;
    }

    public static string GetGivenName(this ClaimsPrincipal cp, string missing = "")
    {
        var claim = cp.Claims.FirstOrDefault(c => c.Type == FabricaClaims.GivenNameClaim);
        return claim?.Value ?? missing;
    }

    public static string GetFamilyName(this ClaimsPrincipal cp, string missing = "")
    {
        var claim = cp.Claims.FirstOrDefault(c => c.Type == FabricaClaims.FamilyNameClaim);
        return claim?.Value ?? missing;
    }

    public static string GetName(this ClaimsPrincipal cp, string missing = "")
    {
        var claim = cp.Claims.FirstOrDefault(c => c.Type == FabricaClaims.NameClaim);
        return claim?.Value ?? missing;
    }

    public static string GetEmail(this ClaimsPrincipal cp, string missing = "")
    {
        var claim = cp.Claims.FirstOrDefault(c => c.Type == FabricaClaims.EmailClaim);
        return claim?.Value ?? missing;
    }

    public static string GetPictureUrl(this ClaimsPrincipal cp, string missing = "")
    {
        var claim = cp.Claims.FirstOrDefault(c => c.Type == FabricaClaims.PictureClaim);
        return claim?.Value ?? missing;
    }


    public static IEnumerable<string> GetRoles(this ClaimsPrincipal cp )
    {
        var roles = cp.Claims.Where(c => c.Type == FabricaClaims.RoleClaim);
        return roles.Select(c => c.Value);
    }






}