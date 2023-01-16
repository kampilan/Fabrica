using Fabrica.Watch;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Threading;

namespace Fabrica.Make.Sdk
{


    public class MakeRequestHandler: DelegatingHandler
    {

        public MakeRequestHandler( string token )
        {
            Token = token;
        }

        public string Token { get; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            using var logger = this.EnterMethod();
                

            // *****************************************************************
            logger.Debug("Attempting to add Authorization header");
            request.Headers.Authorization = new AuthenticationHeaderValue("Token", Token);



            // *****************************************************************
            logger.Debug("Attempting to Send request");
            var response = await base.SendAsync(request, cancellationToken);



            // *****************************************************************
            return response;


        }



    }


}
