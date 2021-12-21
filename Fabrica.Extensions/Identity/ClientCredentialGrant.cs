using System.Collections.Generic;

namespace Fabrica.Identity;

public class ClientCredentialGrant : ICredentialGrant
{


    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";

    public string Audience { get; set; } = "";

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

        if (!string.IsNullOrWhiteSpace(Audience))
            dict["audience"] = Audience;


        return dict;

    }



}