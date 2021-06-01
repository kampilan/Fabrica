using System.Collections.Generic;
using Fabrica.Identity;

namespace Fabrica.Api.Support.Identity.Key
{

    
    public class ApiKeyService
    {

        public string ApiKey { get; set; } = "";

        public string AuthenticationType { get; set; } = "ApiKey";
        public string Tenant { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Picture { get; set; } = "";
        public IEnumerable<string> Roles { get; set; } = new List<string>();

        public bool Validate(string apiKey, out IClaimSet claims )
        {

            claims = null;

            if( apiKey != ApiKey )
                return false;

            claims = new ClaimSetModel
            {
                AuthenticationType = AuthenticationType,
                Tenant             = Tenant,
                Subject            = Subject,
                Name               = Name,
                Email              = Email,
                Picture            = Picture,
                Roles              = new List<string>(Roles)
            };

            return true;

        }





    }


}
