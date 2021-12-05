using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Fabrica.Utilities.Container;
using FileHelpers;
using JetBrains.Annotations;

namespace Fabrica.Api.Support.Controllers;


public abstract class BaseEtlController: BaseController
{


    protected BaseEtlController(ICorrelation correlation, IMapper mapper ) : base(correlation)
    {

        Mapper = mapper;

    }

    private IMapper Mapper { get; }


    protected async Task ProcessStream<TSpec>( [NotNull] Stream inbound, [NotNull] Func<TSpec,Task> sink, bool stopOnError=true ) where TSpec : class
    {

        if (inbound == null) throw new ArgumentNullException(nameof(inbound));
        if (sink == null) throw new ArgumentNullException(nameof(sink));

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to build processing engine");
        var engine = new FileHelperAsyncEngine<TSpec>();



        // *****************************************************************
        logger.Debug("Attempting to process each inbound record");
        using (var reader = new StreamReader(inbound, leaveOpen: true) )
        using( engine.BeginReadStream(reader) )
        {

            foreach (var spec in engine)
            {

                if( logger.IsTraceEnabled )
                    logger.LogObject(nameof(spec), spec);

                try
                {
                    await sink(spec);
                }
                catch (Exception cause)
                {
                    logger.ErrorWithContext( cause, spec, "Caught Exception processing inbound file" );
                    if( stopOnError)
                        throw;
                }

            }

        }

    }


    protected async Task ProcessStream<TSpec,TTarget>( [NotNull] Stream inbound, [NotNull] Func<TTarget, Task> sink, bool stopOnError = true ) where TSpec : class where TTarget: class
    {

        if (inbound == null) throw new ArgumentNullException(nameof(inbound));
        if (sink == null) throw new ArgumentNullException(nameof(sink));

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to build processing engine");
        var engine = new FileHelperAsyncEngine<TSpec>();



        // *****************************************************************
        logger.Debug("Attempting to process each inbound record");
        using( var reader = new StreamReader(inbound, leaveOpen: true) )
        using( engine.BeginReadStream( reader ) )
        {

            foreach( var spec in engine )
            {

                if( logger.IsTraceEnabled )
                    logger.LogObject(nameof(spec), spec);


                try
                {
                    var target = Mapper.Map<TTarget>(spec);
                    await sink(target);
                }
                catch (Exception cause)
                {
                    logger.ErrorWithContext(cause, spec, "Caught Exception processing inbound file");
                    if (stopOnError)
                        throw;
                }

            }

        }

    }


    protected List<TTarget> ProcessStream<TSpec, TTarget>( [NotNull] Stream inbound, bool stopOnError = true ) where TSpec : class where TTarget : class
    {

        if (inbound == null) throw new ArgumentNullException(nameof(inbound));

        using var logger = EnterMethod();


        var results = new List<TTarget>();


        // *****************************************************************
        logger.Debug("Attempting to build processing engine");
        var engine = new FileHelperAsyncEngine<TSpec>();


        // *****************************************************************
        logger.Debug("Attempting to process each inbound record");
        using (var reader = new StreamReader(inbound, leaveOpen: true))
        using (engine.BeginReadStream(reader))
        {

            foreach (var spec in engine)
            {

                if (logger.IsTraceEnabled)
                    logger.LogObject(nameof(spec), spec);


                try
                {
                    var target = Mapper.Map<TTarget>(spec);
                    results.Add(target);
                }
                catch (Exception cause)
                {
                    logger.ErrorWithContext(cause, spec, "Caught Exception processing inbound file");
                    if (stopOnError)
                        throw;
                }

            }

        }



        // *****************************************************************
        logger.Inspect(nameof(results.Count), results.Count);
        return results;


    }


}
