using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Threading;
using Newtonsoft.Json;

namespace Fabrica.Identity
{

    public interface IOidcConfiguration
    {

        string OidcMetaEndpoint { get; set; }

        string OidcClientId { get; set; }
        string OidcClientSecret { get; set; }

        string OidcAudience { get; set; }

    }

    public interface IAccessTokenSource
    {

        bool HasExpired { get; }
        Task<string> GetToken();

    }



    public class OidcAccessTokenSource : CorrelatedObject, IAccessTokenSource, IStartable
    {


        public OidcAccessTokenSource( IOidcConfiguration config, IHttpClientFactory factory, ICorrelation correlation ) : base(correlation)
        {

            Config  = config;
            Factory = factory;

            Credentials = new Dictionary<string, string>
            {
                ["grant_type"]    = "client_credentials",
                ["client_id"]     = Config.OidcClientId,
                ["client_secret"] = Config.OidcClientSecret
            };

        }



        private IOidcConfiguration Config { get; }
        private IHttpClientFactory Factory { get; }

        private MetaModel Meta { get; set; }
        private TokenModel Token { get; set; }

        private IDictionary<string, string> Credentials { get; }



        public void Start()
        {

            using var logger = EnterMethod();

            AsyncPump.Run(StartAsync);

        }

        protected async Task StartAsync()
        {

            using var logger = EnterMethod();

            Meta  = await _fetchMeta();
            Token = await _fetchToken();

        }


        public bool HasExpired => Token?.HasExpired() ?? true;

        public Task<string> GetToken()
        {

            using var logger = EnterMethod();

            var token = Token.AccessToken ?? "";

            return Task.FromResult(token);

        }


        private async Task<MetaModel> _fetchMeta()
        {

            using var logger = EnterMethod();


            using var client = Factory.CreateClient("OidcEndpoint");

            try
            {


                // *****************************************************************
                logger.Debug("Attempting to fetch Meta");
                var res = await client.GetAsync( Config.OidcMetaEndpoint );

                logger.Inspect(nameof(res.StatusCode), res.StatusCode);

                res.EnsureSuccessStatusCode();



                // *****************************************************************
                logger.Debug("Attempting to build read JSON");
                var json = await res.Content.ReadAsStringAsync();

                logger.LogJson("Meta JSON", json);



                // *****************************************************************
                logger.Debug("Attempting to build MetaModel from JSON");
                var model = JsonConvert.DeserializeObject<MetaModel>(json);

                if (model == null)
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


        private async Task<TokenModel> _fetchToken()
        {

            using var logger = EnterMethod();


            using var client = Factory.CreateClient("OidcEndpoint");
            try
            {


                // *****************************************************************
                logger.Debug("Attempting to build credentials");
                var content = new FormUrlEncodedContent(Credentials);



                // *****************************************************************
                logger.Debug("Attempting to fetch tokens form identity provider");
                var res = await client.PostAsync(Meta.TokenEndpoint, content);

                logger.Inspect(nameof(res.StatusCode), res.StatusCode);

                res.EnsureSuccessStatusCode();



                // *****************************************************************
                logger.Debug("Attempting to get token JSON from response");
                var json = await res.Content.ReadAsStringAsync();



                // *****************************************************************
                logger.Debug("Attempting to build TokenModel");
                var model = JsonConvert.DeserializeObject<TokenModel>(json);

                if (model == null)
                    throw new Exception("Null TokenModel encountered");

                logger.LogObject(nameof(model), model);



                // *****************************************************************
                return model;

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


        [JsonProperty("issuer")]
        public string Issuer { get; set; } = "";

        [JsonProperty("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; } = "";

        [JsonProperty("token_endpoint")]
        public string TokenEndpoint { get; set; } = "";

        [JsonProperty("introspection_endpoint")]
        public string IntrospectionEndpoint { get; set; } = "";

        [JsonProperty("userinfo_endpoint")]
        public string UserInfoEndpoint { get; set; } = "";


    }


    public class TokenModel
    {


        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = "";

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = "";


        private DateTime _created = DateTime.Now;
        public TimeSpan GetAccessTokenTtl()
        {
            var ts = (DateTime.Now - _created) - TimeSpan.FromSeconds(120);
            return ts;
        }

        public DateTime GetExpiration()
        {
            var ts = _created + TimeSpan.FromSeconds( ExpiresIn - 120 );
            return ts;
        }

        public bool HasExpired()
        {
            return GetExpiration() <= DateTime.Now;
        }



    }






}
