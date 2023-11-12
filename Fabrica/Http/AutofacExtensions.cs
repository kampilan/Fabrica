// ReSharper disable UnusedMember.Global

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Fabrica.Identity;
using Fabrica.Utilities.Container;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace Fabrica.Http;


public class CookieApiOptions
{

    public string ApiEndpoint { get; set; } = "";

    public int ApiCallRetryCount { get; set; } = 5;

}


public enum TokenApiGrantType { ClientCredential, ResourceOwner }

public class TokenApiOptions
{

    public TokenApiGrantType GrantType { get; set; } = TokenApiGrantType.ClientCredential;

    public string MetaEndpoint { get; set; } = "";

    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";

    public Dictionary<string, string> Additional { get; } = new();

    public string ApiEndpoint { get; set; } = "";

    public int ApiCallRetryCount { get; set; } = 5;


}

public static class AutofacExtensions
{

    public static ContainerBuilder AddCookieApiClient(this ContainerBuilder builder, string name, Action<CookieApiOptions> optionsBuilder )
    {

        var options = new CookieApiOptions();
        optionsBuilder( options );


        // ****************************************************************************************

        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), options.ApiCallRetryCount, fastFirst: true);
        var retry = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay);


        var services = new ServiceCollection();

        services.AddHttpClient(name, c =>
            {

                if (!string.IsNullOrWhiteSpace(options.ApiEndpoint) && options.ApiEndpoint.EndsWith("/"))
                    c.BaseAddress = new Uri(options.ApiEndpoint);
                else if (!string.IsNullOrWhiteSpace(options.ApiEndpoint))
                    c.BaseAddress = new Uri($"{options.ApiEndpoint}/");

            })
            .AddPolicyHandler(retry);


        builder.Populate(services);

        return builder;

    }


    public static ContainerBuilder AddTokenApiClient(this ContainerBuilder builder, string name, Action<TokenApiOptions> optionsBuilder )
    {


        var options = new TokenApiOptions();
        optionsBuilder(options);



        // ****************************************************************************************
        ICredentialGrant grantInstance;

        if( options.GrantType == TokenApiGrantType.ClientCredential )
        {

            var cc = new ClientCredentialGrant
            {
                MetaEndpoint = options.MetaEndpoint,
                ClientId = options.ClientId,
                ClientSecret = options.ClientSecret
            };

            foreach (var p in options.Additional)
                cc.Additional[p.Key] = p.Value;

            grantInstance = cc;

        }
        else if( options.GrantType == TokenApiGrantType.ResourceOwner )
        {

            var ro = new ResourceOwnerGrant
            {
                MetaEndpoint = options.MetaEndpoint,
                UserName = options.UserName,
                Password = options.Password,
                ClientId = options.ClientId,
                ClientSecret = options.ClientSecret
            };

            foreach (var p in options.Additional)
                ro.Additional[p.Key] = p.Value;

            grantInstance = ro;

        }
        else
            throw new InvalidOperationException("Invalid Grant Type specified in TokenApiOptions");





        builder.RegisterInstance(grantInstance)
            .AsSelf()
            .As<ICredentialGrant>()
            .Named<ICredentialGrant>(name)
            .SingleInstance();



        // ****************************************************************************************

        builder.Register(c =>
            {

                var corr    = c.Resolve<ICorrelation>();
                var factory = c.Resolve<IHttpClientFactory>();
                var grant   = c.ResolveNamed<ICredentialGrant>(name);

                var comp = new OidcAccessTokenSource(corr, factory, grant);

                return comp;

            })
            .As<IAccessTokenSource>()
            .Named<IAccessTokenSource>(name)
            .As<IRequiresStart>()
            .SingleInstance();



        // ****************************************************************************************

        builder.Register(c =>
            {

                var source = c.ResolveNamed<IAccessTokenSource>(name);
                var comp   = new AccessTokenSourceRequestHandler(source);

                return comp;

            })
            .AsSelf()
            .InstancePerDependency();



        // ****************************************************************************************

        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), options.ApiCallRetryCount, fastFirst: true);
        var retry = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay);


        var services = new ServiceCollection();

        services.AddHttpClient(name, c =>
            {

                if (!string.IsNullOrWhiteSpace(options.ApiEndpoint) && options.ApiEndpoint.EndsWith("/") )
                    c.BaseAddress = new Uri(options.ApiEndpoint);
                else if( !string.IsNullOrWhiteSpace(options.ApiEndpoint) )
                    c.BaseAddress = new Uri($"{options.ApiEndpoint}/" );

            })
            .AddHttpMessageHandler<AccessTokenSourceRequestHandler>()
            .AddPolicyHandler(retry);


        builder.Populate(services);


        return builder;

    }


    public static ContainerBuilder AddAccessTokenRequestHandler( this ContainerBuilder builder, string name )
    {

        builder.Register(c =>
            {
                var source = c.ResolveNamed<IAccessTokenSource>(name);
                var comp = new AccessTokenSourceRequestHandler(source);
                return comp;
            })
            .AsSelf()
            .InstancePerDependency();

        return builder;

    }



    public static ContainerBuilder AddHttpClient( this ContainerBuilder builder, string name, string baseUri="", int retryCount=5 )
    {


        var delay = Backoff.DecorrelatedJitterBackoffV2( TimeSpan.FromSeconds(1), retryCount, fastFirst: true );
        var retry = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync( delay );


        var services = new ServiceCollection();

        services.AddHttpClient(name, c =>
            {

                if( !string.IsNullOrWhiteSpace(baseUri) )
                    c.BaseAddress = new Uri(baseUri);

            })
            .AddPolicyHandler(retry);


        builder.Populate(services);


        return builder;

    }


    public static ContainerBuilder AddHttpClient<THandler>(this ContainerBuilder builder, string name, string baseUri = "", int retryCount = 5) where THandler: DelegatingHandler
    {


        var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), retryCount, fastFirst: true);
        var retry = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(delay);


        var services = new ServiceCollection();

        services.AddHttpClient(name, c =>
            {

                if (!string.IsNullOrWhiteSpace(baseUri))
                    c.BaseAddress = new Uri(baseUri);

            })
            .AddHttpMessageHandler<THandler>()
            .AddPolicyHandler(retry);


        builder.Populate(services);


        return builder;

    }


}