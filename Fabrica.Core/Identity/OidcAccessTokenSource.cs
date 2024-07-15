using System.Text.Json;
using System.Text.Json.Serialization;
using Fabrica.Utilities.Cache;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.Identity;

public class OidcAccessTokenSource : CorrelatedObject, IAccessTokenSource, IRequiresStart
{


    public OidcAccessTokenSource( ICorrelation correlation, IHttpClientFactory factory, ICredentialGrant grant ) : base(correlation)
    {

        Factory = factory;
        Grant   = grant;

        _cache = new ConcurrentResource<TokenModel>( _fetchToken );

    }

    public TimeSpan RenewalWindow { get; set; } = TimeSpan.FromSeconds(30);

    private IHttpClientFactory Factory { get; }
    private ICredentialGrant Grant { get; }

    private MetaModel? Meta { get; set; }

    public string Name => Grant.Name;

    public async Task Start()
    {

        using var logger = EnterMethod();

        Meta  = await _fetchMeta();

        await _cache.Initialize();

    }

    private readonly ConcurrentResource<TokenModel> _cache;
    public bool HasExpired => _cache.HasExpired;
    public int RenewCount => _cache.RenewCount;

    public async Task<string> GetToken()
    {

        var resource = await _cache.GetResource();

        var token = resource.AccessToken;

        return token;

    }


    private async Task<MetaModel> _fetchMeta()
    {

        using var logger = EnterMethod();


        using var client = Factory.CreateClient();

        try
        {

            if( !string.IsNullOrWhiteSpace(Grant.TokenEndpoint) )
            {
                var meta = new MetaModel { TokenEndpoint = Grant.TokenEndpoint };
                return meta;
            }


            // *****************************************************************
            logger.Debug("Attempting to fetch Meta");
            var res = await client.GetAsync( Grant.MetaEndpoint );

            logger.Inspect(nameof(res.StatusCode), res.StatusCode);

            res.EnsureSuccessStatusCode();



            // *****************************************************************
            logger.Debug("Attempting to build read JSON");
            var json = await res.Content.ReadAsStringAsync();

            logger.LogJson("Meta JSON", json);



            // *****************************************************************
            logger.Debug("Attempting to build MetaModel from JSON");
            var model = JsonSerializer.Deserialize<MetaModel>(json);

            if (model is null)
                throw new Exception("Null MetaModel encountered");

            logger.LogObject(nameof(model), model);



            // *****************************************************************
            return model;


        }
        catch (Exception cause)
        {
            logger.Error(cause, "Get Meta failed");
            throw;
        }

    }


    private async Task<IRenewedResource<TokenModel>> _fetchToken()
    {

        using var logger = EnterMethod();


        using var client = Factory.CreateClient();
        try
        {


            // *****************************************************************
            logger.Debug("Attempting to build credentials");
            var content = new FormUrlEncodedContent(Grant.Body);



            // *****************************************************************
            logger.Debug("Attempting to fetch tokens form identity provider");
            var res = await client.PostAsync(Meta?.TokenEndpoint, content);

            logger.Inspect(nameof(res.StatusCode), res.StatusCode);

            res.EnsureSuccessStatusCode();



            // *****************************************************************
            logger.Debug("Attempting to get token JSON from response");
            var json = await res.Content.ReadAsStringAsync();



            // *****************************************************************
            logger.Debug("Attempting to build TokenModel");
            var model = JsonSerializer.Deserialize<TokenModel>(json);

            if (model is null)
                throw new Exception("Null TokenModel encountered");

            logger.LogObject(nameof(model), model);


            var timeToLive = model.GetAccessTokenTtl();
            var timeToRenew = timeToLive - RenewalWindow;


            // *****************************************************************
            return new RenewedResource<TokenModel>{ Value = model, TimeToLive = timeToLive, TimeToRenew = timeToRenew };

        }
        catch (Exception cause)
        {
            logger.Error(cause, "Get Token failed");
            throw;
        }


    }


}


public class MetaModel
{


    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = "";

    [JsonPropertyName("authorization_endpoint")]
    public string AuthorizationEndpoint { get; set; } = "";

    [JsonPropertyName("token_endpoint")]
    public string TokenEndpoint { get; set; } = "";

    [JsonPropertyName("introspection_endpoint")]
    public string IntrospectionEndpoint { get; set; } = "";

    [JsonPropertyName("userinfo_endpoint")]
    public string UserInfoEndpoint { get; set; } = "";


}


public class TokenModel
{


    [Sensitive]
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [Sensitive]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = "";


    private readonly DateTime _created = DateTime.Now;
    public TimeSpan GetAccessTokenTtl()
    {
        var ts = TimeSpan.FromSeconds(ExpiresIn);
        return ts;
    }

    public DateTime GetExpiration()
    {
        var dt = _created + GetAccessTokenTtl();
        return dt;
    }

    public bool HasExpired()
    {
        return GetExpiration() <= DateTime.Now;
    }



}