using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fabrica.Api.Support.Identity.Token;
using Fabrica.Identity;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fabrica.Api.Support.Identity.Proxy
{



    public static class ServiceCollectionExtensions
    {


        public static IServiceCollection AddProxyTokenAuthentication(this IServiceCollection services)
        {

            services.AddAuthentication(op =>
                {
                    op.DefaultScheme = TokenConstants.Scheme;
                })
                .AddProxyToken();

            return services;

        }

    }

    public static class AuthenticationBuilderExtensions
    {

        public static AuthenticationBuilder AddProxyToken( this AuthenticationBuilder builder )
        {

            builder.AddScheme<TokenAuthenticationSchemeOptions, TokenAuthenticationHandler>( TokenConstants.Scheme, op => { } );

            return builder;

        }

    }


    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationSchemeOptions>
    {


        public TokenAuthenticationHandler( ICorrelation correlation, IProxyTokenEncoder jwtEncoder, IOptionsMonitor<TokenAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock )
        {

            Correlation = correlation;
            JwtEncoder  = jwtEncoder;

        }

        private ICorrelation Correlation { get; }
        private IProxyTokenEncoder JwtEncoder { get; }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            using var logger = this.EnterMethod();


            var token = Context.Request.Headers[TokenConstants.HeaderName].FirstOrDefault();
            if( string.IsNullOrWhiteSpace(token) )
            {
                logger.Debug("Header not present. Attempting to build skip result");
                var noresult = AuthenticateResult.NoResult();
                return Task.FromResult(noresult);
            }



            IClaimSet claims;
            try
            {
                claims = JwtEncoder.Decode( TokenConstants.Scheme, token );
            }
            catch (Exception cause)
            {
                logger.Debug( cause, "Decode failed. Attempting to build skip result" );
                var noresult = AuthenticateResult.NoResult();
                return Task.FromResult(noresult);
            }



            // *****************************************************************
            logger.Debug("Attempting to build ClaimsIdentity");
            var ci = new FabricaIdentity( claims );
            ci.Populate( claims );



            // *****************************************************************
            logger.Debug("Attempting to build ClaimsPrincipal");
            var cp = new ClaimsPrincipal(ci);



            // *****************************************************************
            logger.Debug("Attempting to build ticket and success result");
            var ticket = new AuthenticationTicket( cp, new AuthenticationProperties(), TokenConstants.Scheme );
            var result = AuthenticateResult.Success(ticket);



            // *****************************************************************
            logger.Debug("Attempting to set Caller on Correlation");
            if( Correlation is Correlation impl )
                impl.Caller = result.Principal;



            // *****************************************************************
            return Task.FromResult(result);


        }


    }


}
