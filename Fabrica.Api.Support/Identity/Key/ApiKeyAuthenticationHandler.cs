using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fabrica.Identity;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fabrica.Api.Support.Identity.Key
{



    public static class AuthenticationBuilderExtensions
    {

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder)
        {

            builder.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", op => { });

            return builder;

        }

    }


    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {


        public ApiKeyAuthenticationHandler( ICorrelation correlation, ApiKeyService service, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {

            Correlation = correlation;
            Service     = service;

        }

        private ICorrelation Correlation { get; }
        private ApiKeyService Service { get; }


        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            using var logger = this.EnterMethod();


            var key = Context.Request.Headers["X-Api-Key"].FirstOrDefault();
            if( string.IsNullOrWhiteSpace(key) )
            {
                logger.Debug("Header not present. Attempting to build skip result");
                var noresult = AuthenticateResult.NoResult();
                return Task.FromResult(noresult);
            }


            if( Service.Validate(key, out var claims) )
            {


                // *****************************************************************
                logger.Debug("Attempting to build ClaimsIdentity");
                var ci = new FabricaIdentity(claims);
                ci.Populate(claims);



                // *****************************************************************
                logger.Debug("Attempting to build ClaimsPrincipal");
                var cp = new ClaimsPrincipal(ci);



                // *****************************************************************
                logger.Debug("Attempting to build ticket and success result");
                var ticket = new AuthenticationTicket(cp, new AuthenticationProperties(), "ApiKey");
                var result = AuthenticateResult.Success(ticket);



                // *****************************************************************
                logger.Debug("Attempting to set Caller on Correlation");
                if (Correlation is Correlation impl)
                    impl.Caller = result.Principal;



                // *****************************************************************
                return Task.FromResult(result);

            }
            else
            {
                logger.Debug("Keys don't match Attempting to build skip result");
                var noresult = AuthenticateResult.NoResult();
                return Task.FromResult( noresult );
            }


        }


    }


}
