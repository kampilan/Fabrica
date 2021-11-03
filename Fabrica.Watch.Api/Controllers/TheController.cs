using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Fabrica.Api.Support.Controllers;
using Fabrica.Utilities.Container;
using Fabrica.Watch.Api.Appliance;
using Fabrica.Watch.Mongo;
using Fabrica.Watch.Sink;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static System.Enum;

namespace Fabrica.Watch.Api.Controllers
{


    [Route("/")]
    public class TheController: BaseController
    {


        public TheController(ICorrelation correlation, WatchOptions options ) : base(correlation)
        {

            Options = options;

        }


        private WatchOptions Options { get; }


        [HttpPost]
        [SwaggerOperation(Summary = "Create", Description = "Create Log Event")]
        [SwaggerResponse(200, "Success")]
        public async Task<StatusCodeResult> Post( [FromQuery] LogModel model )
        {


            using var logger = EnterMethod();

            logger.LogObject(nameof(Options), Options);
            logger.LogObject(nameof(model), model);


            if (string.IsNullOrWhiteSpace(Options.WatchEventStoreUri))
                return BadRequest();



            // *****************************************************************
            logger.Debug("Attempting to build LogEvent components");
            if (!TryParse(model.Level, out Level level))
                level = Level.Error;

            if (!TryParse(model.PayloadType, out PayloadType pt))
                pt = PayloadType.Text;

            using var reader  = new StreamReader(Request.Body, leaveOpen: true);
            var payload = await reader.ReadToEndAsync();

            var domain = model.Domain;
            if (string.IsNullOrWhiteSpace(domain))
                domain = Options.DefaultWatchDomain;

            logger.Inspect(nameof(domain), domain);



            // *****************************************************************
            logger.Debug("Attempting to build Watch Factory");
            var maker = new WatchFactoryBuilder();
            maker.UseMongo( Options.WatchEventStoreUri, domain, false );



            // *****************************************************************
            logger.Debug("Attempting to start Watch Factory");
            var factory = maker.BuildNoSet();
            factory.Start();

            try
            {

                // *****************************************************************
                logger.Debug("Attempting to get logger for request Category");
                var exLogger = factory.GetLogger(model.Category);

                logger.Inspect(nameof(exLogger.IsTraceEnabled), exLogger.IsTraceEnabled);
                logger.Inspect(nameof(exLogger.IsDebugEnabled), exLogger.IsDebugEnabled);
                logger.Inspect(nameof(exLogger.IsInfoEnabled), exLogger.IsInfoEnabled);
                logger.Inspect(nameof(exLogger.IsWarningEnabled), exLogger.IsWarningEnabled);
                logger.Inspect(nameof(exLogger.IsErrorEnabled), exLogger.IsErrorEnabled);

                if ((level == Level.Error && exLogger.IsErrorEnabled) ||
                    (level == Level.Warning && exLogger.IsWarningEnabled) ||
                    (level == Level.Info && exLogger.IsInfoEnabled) ||
                    (level == Level.Debug && exLogger.IsDebugEnabled) ||
                    (level == Level.Trace && exLogger.IsTraceEnabled))
                {

                    // *****************************************************************
                    logger.Debug("Attempting to create and log event");
                    var le = exLogger.CreateEvent(level, model.Title, pt, payload);

                    exLogger.LogEvent(le);

                }


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
            logger.LogObject(nameof(Options), Options);


            if (string.IsNullOrWhiteSpace(Options.WatchEventStoreUri))
                return Task.FromResult( (StatusCodeResult)BadRequest() );



            // *****************************************************************
            logger.Debug("Attempting to build LogEvent components");
            if (!TryParse(model.Level, out Level level))
                level = Level.Error;

            if (!TryParse(model.PayloadType, out PayloadType pt))
                pt = PayloadType.Text;

            var buff    = Convert.FromBase64String(model.Payload);
            var payload = Encoding.UTF8.GetString(buff);



            var domain = model.Domain;
            if( string.IsNullOrWhiteSpace(domain) )
                domain = Options.DefaultWatchDomain;

            logger.Inspect(nameof(domain), domain);


            // *****************************************************************
            logger.Debug("Attempting to build Watch Factory");
            var maker = new WatchFactoryBuilder();
            maker.UseMongo(Options.WatchEventStoreUri, domain, false);



            // *****************************************************************
            logger.Debug("Attempting to start Watch Factory");
            var factory = maker.BuildNoSet();
            factory.Start();

            try
            {


                // *****************************************************************
                logger.Debug("Attempting to get logger for request Category");
                var exLogger = factory.GetLogger(model.Category);


                logger.Inspect(nameof(exLogger.IsTraceEnabled), exLogger.IsTraceEnabled);
                logger.Inspect(nameof(exLogger.IsDebugEnabled), exLogger.IsDebugEnabled);
                logger.Inspect(nameof(exLogger.IsInfoEnabled), exLogger.IsInfoEnabled);
                logger.Inspect(nameof(exLogger.IsWarningEnabled), exLogger.IsWarningEnabled);
                logger.Inspect(nameof(exLogger.IsErrorEnabled), exLogger.IsErrorEnabled);


                if ((level == Level.Error && exLogger.IsErrorEnabled) ||
                    (level == Level.Warning && exLogger.IsWarningEnabled) ||
                    (level == Level.Info && exLogger.IsInfoEnabled) ||
                    (level == Level.Debug && exLogger.IsDebugEnabled) ||
                    (level == Level.Trace && exLogger.IsTraceEnabled))
                {

                    // *****************************************************************
                    logger.Debug("Attempting to create and log event");
                    var le = exLogger.CreateEvent(level, model.Title, pt, payload);

                    exLogger.LogEvent(le);

                }



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
