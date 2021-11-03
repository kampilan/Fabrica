using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.Api.Support.Controllers;
using Fabrica.Utilities.Container;
using Fabrica.Watch.Mongo;
using Fabrica.Watch.Mongo.Sink;
using Fabrica.Watch.Sink;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Fabrica.Watch.Api.Controllers
{

    [Route("/")]
    public class TheController: BaseController
    {


        public TheController(ICorrelation correlation) : base(correlation)
        {
        }


        [HttpPost]
        [SwaggerOperation(Summary = "Create", Description = "Create Billing Request")]
        [SwaggerResponse(200, "Success")]
        public async Task<StatusCodeResult> Post( [FromQuery] LogModel model )
        {


            using var logger = EnterMethod();

            logger.LogObject(nameof(model), model);


            var serverUri = "";
            if( WatchFactoryLocator.Factory.Sink is CompositeSink root )
            {
                foreach( var sink in root.InnerSinks )
                    if (sink is MongoEventSink mongo)
                        serverUri = mongo.ServerUri;
            }

            if (string.IsNullOrWhiteSpace(serverUri))
                return BadRequest();



            var maker = new WatchFactoryBuilder();
            maker.UseMongo( serverUri, model.Domain, false );

            var factory = maker.BuildNoSet();
            factory.Start();


            Enum.TryParse(model.Level, out Level level );
            Enum.TryParse(model.PayloadType, out PayloadType pt);

            var reader = new StreamReader(Request.Body);

            var payload = await reader.ReadToEndAsync();



            var exLogger = factory.GetLogger(model.Category);

            var le = exLogger.CreateEvent(level, model.Title, pt, payload);

            exLogger.LogEvent( le );

            factory.Stop();


            return Ok();

        }


    }


    public class LogModel
    {

        public string Domain { get; set; } = "";

        public string Category { get; set; } = "";
        public string Level { get; set; } = "";

        public string Title { get; set; } = "";

        public string PayloadType { get; set; } = "";


    }    



}
