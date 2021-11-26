using System.Collections.Generic;
using System.Threading.Tasks;
using Fabrica.Api.Support.Controllers;
using Fabrica.Utilities.Container;
using Fabrica.Watch.Api.Components;
using Fabrica.Watch.Sink;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Watch.Api.Controllers
{


    [Route("/")]
    public class TheController : BaseController
    {

        public TheController(ICorrelation correlation, WatchSinkCache cache) : base(correlation)
        {

            Cache = cache;

        }

        private WatchSinkCache Cache { get; }


        [HttpPost("{domain}")]
        [SwaggerOperation(Summary = "Create", Description = "Create Log Event Batch from JSON body")]
        [SwaggerResponse(200, "Success")]
        public async Task<StatusCodeResult> PostBatch([FromRoute] string domain, [FromBody] List<LogEvent> batch)
        {


            using var logger = EnterMethod();

            logger.Inspect(nameof(domain), domain);
            logger.Inspect(nameof(batch.Count), batch.Count);


            // *****************************************************************
            logger.Debug("Attempting to start get Event Sink from Cache");
            var sink = Cache.GetSink(domain);

            await sink.Accept(batch);



            // *****************************************************************
            return Ok();


        }



    }


}
