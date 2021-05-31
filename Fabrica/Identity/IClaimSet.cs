using System.Collections.Generic;

namespace Fabrica.Identity
{


    public interface IClaimSet
    {

        string AuthenticationType { get; }

        string Tenant { get; }

        string Subject { get; }

        string Name { get; }

        string Email { get; }

        string Picture { get; }

        public IEnumerable<string> Roles { get;}


    }


}