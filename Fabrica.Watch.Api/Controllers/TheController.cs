using System;
using System.Collections.Generic;
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


        [HttpPost("{domain}")]
        [SwaggerOperation(Summary = "Create", Description = "Create Log Event Batch from JSON body")]
        [SwaggerResponse(200, "Success")]
        public Task<StatusCodeResult> PostBatch( [FromRoute] string domain, [FromBody] List<LogModel> batch )
        {


            using var logger = EnterMethod();

            logger.Inspect(nameof(batch.Count), batch.Count);
            logger.LogObject(nameof(Options), Options);


            if (string.IsNullOrWhiteSpace(Options.WatchEventStoreUri))
                return Task.FromResult( (StatusCodeResult)BadRequest() );


            if ( string.IsNullOrWhiteSpace(domain) )
                domain = Options.DefaultWatchDomain;

            logger.Inspect(nameof(domain), domain);


            // *****************************************************************
            logger.Debug("Attempting to build Watch Factory");
            var maker = new WatchFactoryBuilder();
            maker.UseMongo(Options.WatchEventStoreUri, domain);



            // *****************************************************************
            logger.Debug("Attempting to start Watch Factory");
            var factory = maker.BuildNoSet();
            factory.Start();

            try
            {

                foreach( var model in batch )
                {


                    // *****************************************************************
                    logger.Debug("Attempting to build LogEvent components");
                    if (!TryParse(model.Level, out Level level))
                        level = Level.Error;

                    if (!TryParse(model.PayloadType, out PayloadType pt))
                        pt = PayloadType.Text;


                    string payload = "";
                    if( !string.IsNullOrWhiteSpace(model.Payload) )
                    {
                        var buff = Convert.FromBase64String(model.Payload);
                        payload  = Encoding.UTF8.GetString(buff);
                    }


                    // *****************************************************************
                    logger.Debug("Attempting to get logger for request Category");
                    var exLogger = factory.GetLogger(model, false);


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
                        ILogEvent le;
                        if( !string.IsNullOrWhiteSpace(payload) )
                            le = exLogger.CreateEvent(level, model.Title, pt, payload);
                        else
                            le = exLogger.CreateEvent(level, model.Title );

                        exLogger.LogEvent(le);

                    }

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

        public static implicit operator LoggerRequest(LogModel m) => new() { Category = m.Category, CorrelationId = m.CorrelationId, Subject = m.Subject, Tenant = m.Tenant };


        public bool Debug { get; set; } = false;

        public string Tag { get; set; } = "";
        public string Tenant { get; set; } = "";
        public string Subject { get; set; } = "";

        public string CorrelationId { get; set; } = "";

        public string Category { get; set; } = "";

        public string Level { get; set; } = "";

        public string Title { get; set; } = "";

        public string PayloadType { get; set; } = "";

        public string Payload { get; set; } = "";

    }



}
