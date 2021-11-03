﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Fabrica.Api.Support.Controllers;
using Fabrica.Utilities.Container;
using Fabrica.Watch.Mongo;
using Fabrica.Watch.Mongo.Sink;
using Fabrica.Watch.Sink;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static System.Enum;

namespace Fabrica.Watch.Api.Controllers
{


    [Route("/")]
    public class TheController: BaseController
    {


        public TheController(ICorrelation correlation) : base(correlation)
        {
        }


        [HttpPost]
        [SwaggerOperation(Summary = "Create", Description = "Create Log Event")]
        [SwaggerResponse(200, "Success")]
        public async Task<StatusCodeResult> Post( [FromQuery] LogModel model )
        {


            using var logger = EnterMethod();

            logger.LogObject(nameof(model), model);



            // *****************************************************************
            logger.Debug("Attempting to Dig out Mongo Sink");
            var serverUri = "";
            if( WatchFactoryLocator.Factory.Sink is CompositeSink root )
            {
                foreach( var sink in root.InnerSinks )
                    if (sink is MongoEventSink mongo)
                        serverUri = mongo.ServerUri;
            }

            logger.Inspect(nameof(serverUri), serverUri);


            if (string.IsNullOrWhiteSpace(serverUri))
                return BadRequest();



            // *****************************************************************
            logger.Debug("Attempting to build LogEvent components");
            if (!TryParse(model.Level, out Level level))
                level = Level.Error;

            if (!TryParse(model.PayloadType, out PayloadType pt))
                pt = PayloadType.Text;

            using var reader  = new StreamReader(Request.Body, leaveOpen: true);
            var       payload = await reader.ReadToEndAsync();



            // *****************************************************************
            logger.Debug("Attempting to build Watch Factory");
            var maker = new WatchFactoryBuilder();
            maker.UseMongo( serverUri, model.Domain, false );



            // *****************************************************************
            logger.Debug("Attempting to start Watch Factory");
            var factory = maker.BuildNoSet();
            factory.Start();

            try
            {

                // *****************************************************************
                logger.Debug("Attempting to get logger for request Category");
                var exLogger = factory.GetLogger(model.Category);



                // *****************************************************************
                logger.Debug("Attempting to create and log event");
                var le = exLogger.CreateEvent(level, model.Title, pt, payload);

                exLogger.LogEvent(le);



            }
            finally
            {

                // *****************************************************************
                logger.Debug("Attempting to stop factory");
                factory.Stop();

            }



            // *****************************************************************
            return Ok();

        }



        [HttpPost("/json")]
        [SwaggerOperation(Summary = "Create", Description = "Create Log Event from JSON body")]
        [SwaggerResponse(200, "Success")]
        public Task<StatusCodeResult> PostJson([FromBody] LogModelWithBody model)
        {


            using var logger = EnterMethod();

            logger.LogObject(nameof(model), model);



            // *****************************************************************
            logger.Debug("Attempting to dig out Mongo Sink");
            var serverUri = "";
            if( WatchFactoryLocator.Factory.Sink is CompositeSink root )
            {
                foreach (var sink in root.InnerSinks)
                    if (sink is MongoEventSink mongo)
                        serverUri = mongo.ServerUri;
            }

            logger.Inspect(nameof(serverUri), serverUri);


            if (string.IsNullOrWhiteSpace(serverUri))
                return Task.FromResult( (StatusCodeResult)BadRequest() );


            // *****************************************************************
            logger.Debug("Attempting to build LogEvent components");
            if (!TryParse(model.Level, out Level level))
                level = Level.Error;

            if (!TryParse(model.PayloadType, out PayloadType pt))
                pt = PayloadType.Text;

            var buff    = Convert.FromBase64String(model.Payload);
            var payload = Encoding.UTF8.GetString(buff);



            // *****************************************************************
            logger.Debug("Attempting to build Watch Factory");
            var maker = new WatchFactoryBuilder();
            maker.UseMongo(serverUri, model.Domain, false);



            // *****************************************************************
            logger.Debug("Attempting to start Watch Factory");
            var factory = maker.BuildNoSet();
            factory.Start();

            try
            {


                // *****************************************************************
                logger.Debug("Attempting to get logger for request Category");
                var exLogger = factory.GetLogger(model.Category);



                // *****************************************************************
                logger.Debug("Attempting to create and log event");
                var le = exLogger.CreateEvent(level, model.Title, pt, payload);

                exLogger.LogEvent(le);


            }
            finally
            {
                // *****************************************************************
                logger.Debug("Attempting to stop factory");
                factory.Stop();
            }




            // *****************************************************************
            return Task.FromResult((StatusCodeResult)Ok());


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


    public class LogModelWithBody
    {

        public string Domain { get; set; } = "";

        public string Category { get; set; } = "";
        public string Level { get; set; } = "";

        public string Title { get; set; } = "";

        public string PayloadType { get; set; } = "";

        public string Payload { get; set; } = "";


    }




}
