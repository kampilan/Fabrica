using System;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Threading;
using Fabrica.Watch;
using Newtonsoft.Json;

namespace Fabrica.Identity
{


    public interface IAccessTokenSource
    {

        bool HasExpired { get; }
        Task<string> GetToken();

    }

    public class OidcAccessTokenSource : CorrelatedObject, IAccessTokenSource, IStartable
    {


        public OidcAccessTokenSource( ICorrelation correlation, IHttpClientFactory factory, string clientName, ICredentialGrant grant, string metaEndpoint="", string tokenEndpoint="" ) : base(correlation)
        {

            Factory    = factory;
            ClientName = clientName;

            Grant = grant;

            MetaEndpoint  = metaEndpoint;
            TokenEndpoint = tokenEndpoint;

        }


        private IHttpClientFactory Factory { get; }
        private string ClientName { get; }

        private string MetaEndpoint { get; }
        private string TokenEndpoint { get; }

        private ICredentialGrant Grant { get; }


        private MetaModel Meta { get; set; }
        private TokenModel Token { get; set; }



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

        public async Task<string> GetToken()
        {

            using var logger = EnterMethod();


            logger.Inspect(nameof(HasExpired), HasExpired);



            // *****************************************************************
            if ( HasExpired )
            {
                logger.Debug("Attempting to fetch new token");
                await _fetchToken();
            }



            // *****************************************************************
            var token = Token.AccessToken ?? "";

            logger.Inspect(nameof(token.Length), token.Length);



            // *****************************************************************
            return token;


        }


        private async Task<MetaModel> _fetchMeta()
        {

            using var logger = EnterMethod();


            using var client = Factory.CreateClient( ClientName );

            try
            {

                if( !string.IsNullOrWhiteSpace(TokenEndpoint) )
                {
                    var meta = new MetaModel { TokenEndpoint = TokenEndpoint };
                    return meta;
                }


                // *****************************************************************
                logger.Debug("Attempting to fetch Meta");
                var res = await client.GetAsync( MetaEndpoint );

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


            using var client = Factory.CreateClient( ClientName );
            try
            {


                // *****************************************************************
                logger.Debug("Attempting to build credentials");
                var content = new FormUrlEncodedContent(Grant.Body);



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


        [Sensitive]
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = "";

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [Sensitive]
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
