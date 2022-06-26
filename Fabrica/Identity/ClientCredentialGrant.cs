using System.Collections.Generic;

namespace Fabrica.Identity;

public class ClientCredentialGrant : ICredentialGrant
{


    public string Name { get; set; } = "";
    public string MetaEndpoint { get; set; } = "";
    public string TokenEndpoint { get; set; } = "";

    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";


    public IDictionary<string, string> Body => _build();


    private IDictionary<string, string> _build()
    {

        var dict = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials"
        };


        if( !string.IsNullOrWhiteSpace(ClientId) )
            dict["client_id"] = ClientId;

        if( !string.IsNullOrWhiteSpace(ClientSecret) )
            dict["client_secret"] = ClientSecret;

        return dict;

    }



}