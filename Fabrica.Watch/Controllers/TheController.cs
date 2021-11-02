using System.Threading.Tasks;
using Fabrica.Api.Support.Controllers;
using Fabrica.Utilities.Container;
using Fabrica.Watch.Mongo;
using Fabrica.Watch.Mongo.Sink;
using Microsoft.AspNetCore.Mvc;

namespace Fabrica.Watch.Controllers
{

    [Route("/")]
    public class TheController: BaseController
    {


        public TheController(ICorrelation correlation) : base(correlation)
        {
        }



        public async Task<StatusCodeResult> Post( [FromQuery] LogModel model )
        {


            using var logger = EnterMethod();


            if( WatchFactoryLocator.Factory.Sink is not MongoEventSink sink )
                return new StatusCodeResult(500);

            var maker = new WatchFactoryBuilder();
            maker.UseMongo( sink.ServerUri, model.Domain, false );

            
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
