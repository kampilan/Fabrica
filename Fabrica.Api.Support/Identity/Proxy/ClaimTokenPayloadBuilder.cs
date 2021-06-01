using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Claims;
using Fabrica.Identity;
using Fabrica.Watch;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Api.Support.Identity.Proxy
{


    public class ClaimTokenPayloadBuilder: IProxyTokenPayloadBuilder
    {

        public ClaimTokenPayloadBuilder()
        {

            var mappings = new Dictionary<string, string>
            {
                ["tenant"]                  = nameof(ClaimSetModel.Tenant),
                [ClaimTypes.NameIdentifier] = nameof(ClaimSetModel.Subject),
                [ClaimTypes.Role]           = nameof(ClaimSetModel.Roles),
                ["name"]                    = nameof(ClaimSetModel.Name),
                ["picture"]                 = nameof(ClaimSetModel.Picture),
                [ClaimTypes.Email]          = nameof(ClaimSetModel.Email)
            };

            ClaimMap = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(mappings));

        }

        public ClaimTokenPayloadBuilder( IEnumerable<KeyValuePair<string, string>> mappings )
        {

            ClaimMap = new ReadOnlyDictionary<string,string>( new Dictionary<string,string>( mappings ) );

        }

        private IReadOnlyDictionary<string,string> ClaimMap { get; }


        public IClaimSet Build( HttpContext context )
        {

            using var logger = this.EnterMethod();

            var payload = new ClaimSetModel();

            foreach (var claim in context.User.Claims)
            {

                logger.Inspect(nameof(claim.Type), claim.Type);
                logger.Inspect(nameof(claim.Value), claim.Value);

                if( ClaimMap.TryGetValue(claim.Type, out var mapped) )
                {

                    logger.Inspect(nameof(mapped), mapped);

                    switch( mapped )
                    {
                        case nameof(ClaimSetModel.Tenant):
                            payload.Tenant = claim.Value;
                            break;
                        case nameof(ClaimSetModel.Subject):
                            payload.Subject = claim.Value;
                            break;
                        case nameof(ClaimSetModel.Roles):
                            payload.Roles.Add( claim.Value );
                            break;
                        case nameof(ClaimSetModel.Name):
                            payload.Name = claim.Value;
                            break;
                        case nameof(ClaimSetModel.Picture):
                            payload.Picture = claim.Value;
                            break;
                        case nameof(ClaimSetModel.Email):
                            payload.Email = claim.Value;
                            break;
                    }

                }

            }


            logger.LogObject(nameof(payload), payload);


            return payload;

        }


    }


}
