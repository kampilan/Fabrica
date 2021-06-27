using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.Api.Support.Controllers;
using Fabrica.Exceptions;
using Fabrica.Utilities.Container;
using Fabrica.Work.Processor;
using Fabrica.Work.Topics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fabrica.Work.Controllers
{
    
    [Authorize]
    [Route("/work")]
    public class DispatchController: BaseController
    {


        public DispatchController( ICorrelation correlation, ITopicMap map, IWorkDispatcher dipatcher ) : base(correlation)
        {
            Map        = map;
            Dispatcher = dipatcher;

        }

        private ITopicMap Map { get; }
        private IWorkDispatcher Dispatcher { get; }


        protected virtual async Task<JObject> FromBody()
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


                // *****************************************************************
                logger.Debug("Attempting to parse request body");
                var jo = await JObject.LoadAsync(new JsonTextReader(new StreamReader(Request.Body)));


                // *****************************************************************
                return jo;

            }
            finally
            {
                logger.LeaveMethod();
            }

        }




        [HttpPost]
        public async Task<IActionResult> Post( [FromQuery] DispatchOptions options )
        {

            using var logger = EnterMethod();

            logger.LogObject(nameof(options), options);



            // *****************************************************************
            logger.Debug("Attempting to verify a TopicEndpoint exists");

            if( !Map.HasTopic( options.Topic ) )
                throw new NotFoundException($"Topic ({options.Topic}) was not found");



            // *****************************************************************
            logger.Debug("Attempting to parse request body");
            var jo = await JObject.LoadAsync(new JsonTextReader(new StreamReader(Request.Body)));



            // *****************************************************************
            logger.Debug("Attempting to build request");
            var request = new WorkRequest
            {
                Topic   = options.Topic,
                Payload = jo
            };



            // *****************************************************************
            logger.Debug("Attempting to dispatch request");
            var ts = TimeSpan.FromSeconds(options.DelaySecs);
            await Dispatcher.Dispatch( request, ts );



            // *****************************************************************
            return Ok();


        }


    }

    public class DispatchOptions
    {

        public string Topic { get; set; } = "";

        public int DelaySecs { get; set; } = 0;

    }




}
