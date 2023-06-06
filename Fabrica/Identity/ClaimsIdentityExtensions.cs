using System.Security.Claims;
using System.Text.Json;

namespace Fabrica.Identity;

public static class ClaimsIdentityExtensions
{


    public static string ToJson(this ClaimsIdentity ci)
    {

        var payload = ci.ToPayload();
        var json = JsonSerializer.Serialize(ci);

        return json;

    }


    public static void Populate( this ClaimsIdentity ci, string json )
    {
        var payload = JsonSerializer.Deserialize<ClaimSetModel>(json);
        ci.Populate( payload );

    }


    public static void Populate( this ClaimsIdentity ci, IClaimSet claimSet )
    {


        CheckClaim( FabricaClaims.FlowClaim, claimSet.AuthenticationFlow );
        CheckClaim( FabricaClaims.TypeClaim, claimSet.AuthenticationType );

        CheckClaim( FabricaClaims.TenantClaim, claimSet.Tenant );

        CheckClaim( FabricaClaims.SubjectClaim, claimSet.Subject );
        CheckClaim( FabricaClaims.AltSubjectClaim, claimSet.AltSubject );

        CheckClaim( FabricaClaims.UserNameClaim, claimSet.UserName );

        CheckClaim( FabricaClaims.GivenNameClaim, claimSet.GivenName );
        CheckClaim( FabricaClaims.FamilyNameClaim, claimSet.FamilyName );
        CheckClaim( FabricaClaims.NameClaim, claimSet.Name );

        CheckClaim( FabricaClaims.EmailClaim, claimSet.Email );
        CheckClaim( FabricaClaims.PictureClaim, claimSet.Picture );

        foreach (var role in claimSet.Roles)
            CheckClaim(FabricaClaims.RoleClaim, role);

        void CheckClaim(string type, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                ci.AddClaim(new Claim(type, value));
        }

    }


    public static IClaimSet ToPayload(this ClaimsIdentity ci)
    {

        var payload = new ClaimSetModel
        {
            AuthenticationType = ci.GetAuthType(),
            AuthenticationFlow = ci.GetAuthFlow(),
            Tenant             = ci.GetTenant(),
            Subject            = ci.GetSubject(),
            AltSubject         = ci.GetAltSubject(),
            UserName           = ci.GetUserName(),
            GivenName          = ci.GetGivenName(),
            FamilyName         = ci.GetFamilyName(),
            Name               = ci.GetName(),
            Email              = ci.GetEmail(),
            Picture            = ci.GetPictureUrl()
        };

        payload.Roles.AddRange( ci.GetRoles() );

        return payload;

    }


    public static string GetAuthType(this ClaimsIdentity ci, string missing = "")
    {
        var claim = ci.Claims.FirstOrDefault(c => c.Type == FabricaClaims.TypeClaim);
        return claim?.Value ?? missing;
    }


    public static string GetAuthFlow(this ClaimsIdentity ci, string missing = "")
    {
        var claim = ci.Claims.FirstOrDefault(c => c.Type == FabricaClaims.FlowClaim);
        return claim?.Value ?? missing;
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

    public static string GetAltSubject(this ClaimsIdentity ci, string missing = "")
    {
        var claim = ci.Claims.FirstOrDefault(c => c.Type == FabricaClaims.AltSubjectClaim);
        return claim?.Value ?? missing;
    }

    public static string GetUserName(this ClaimsIdentity ci, string missing = "")
    {
        var claim = ci.Claims.FirstOrDefault(c => c.Type == FabricaClaims.UserNameClaim);
        return claim?.Value ?? missing;
    }

    public static string GetGivenName(this ClaimsIdentity ci, string missing = "")
    {
        var claim = ci.Claims.FirstOrDefault(c => c.Type == FabricaClaims.GivenNameClaim);
        return claim?.Value ?? missing;
    }

    public static string GetFamilyName(this ClaimsIdentity ci, string missing = "")
    {
        var claim = ci.Claims.FirstOrDefault(c => c.Type == FabricaClaims.FamilyNameClaim);
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
        var roles = ci.Claims.Where(c => c.Type == FabricaClaims.RoleClaim);
        return roles.Select(c => c.Value);
    }


}