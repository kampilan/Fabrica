using System.Collections.Generic;

namespace Fabrica.Identity
{

    
    public class ClaimSetModel: IClaimSet
    {

        public string AuthenticationType { get; set; } = "";


        public string Tenant { get; set; } = "";

        public string Subject { get; set; } = "";

        public string Name { get; set; } = "";

        public string Email { get; set; } = "";

        public string Picture { get; set; } = "";

        public List<string> Roles { get; set; } = new List<string>();

        IEnumerable<string> IClaimSet.Roles => Roles;


    }


}