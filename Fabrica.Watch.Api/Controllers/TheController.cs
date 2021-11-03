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


            if( WatchFactoryLocator.Factory.Sink is not MongoEventSink sink )
                return new StatusCodeResult(500);


            var maker = new WatchFactoryBuilder();
            maker.UseMongo( sink.ServerUri, model.Domain, false );

            var factory = maker.BuildNoSet();



            Enum.TryParse(model.Level, out Level level );
            Enum.TryParse(model.PayloadType, out PayloadType pt);

            var reader = new StreamReader(Request.Body);

            var payload = await reader.ReadToEndAsync();



            var exLogger = factory.GetLogger(model.Category);

            var le = exLogger.CreateEvent(level, model.Title, pt, payload);

            exLogger.LogEvent( le );



            return new StatusCodeResult(200);

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
