using System.Collections.ObjectModel;

namespace Fabrica.Identity;

public class ResourceOwnerGrant : ICredentialGrant
{

    public string Name { get; set; } = "";

    public string MetaEndpoint { get; set; } = "";
    public string TokenEndpoint { get; set; } = "";


    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";

    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";

    public Dictionary<string, string> Additional { get; } = new();


    public IReadOnlyDictionary<string, string> Body => _build();

    private IReadOnlyDictionary<string, string> _build()
    {

        var dict = new Dictionary<string, string>
        {
            ["grant_type"] = "password"
        };

        if( !string.IsNullOrWhiteSpace(ClientId) )
            dict["client_id"] = ClientId;

        if( !string.IsNullOrWhiteSpace(ClientSecret) )
            dict["client_secret"] = ClientSecret;

        if( !string.IsNullOrWhiteSpace(UserName) )
            dict["username"] = UserName;

        if( !string.IsNullOrWhiteSpace(Password) )
            dict["password"] = Password;

        foreach (var p in Additional)
            dict[p.Key] = p.Value;


        return new ReadOnlyDictionary<string, string>(dict);

    }



}