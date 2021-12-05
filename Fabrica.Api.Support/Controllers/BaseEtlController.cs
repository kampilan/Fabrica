using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using FileHelpers;
using JetBrains.Annotations;

namespace Fabrica.Api.Support.Controllers;


public abstract class BaseEtlController: BaseController
{


    protected BaseEtlController(ICorrelation correlation, IMapper mapper, IRuleSet rules ) : base(correlation)
    {

        Mapper = mapper;
        Rules  = rules;

    }

    private IMapper Mapper { get; }
    private IRuleSet Rules { get; }


    protected EvaluationResults Evaluate( params object[] facts )
    {

        var ec = Rules.GetEvaluationContext();
        ec.AddAllFacts(facts);
        ec.ThrowNoRulesException = false;

        var er = Rules.Evaluate(ec);

        return er;

    }


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
                    Evaluate(spec);
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

                    Evaluate(spec, target);

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

                    Evaluate(spec, target);

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


    protected void ProduceStream<TSpec>( Stream outbound, IEnumerable<TSpec> sources ) where TSpec : class
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to build filer helper engine");
        var engine = new FileHelperAsyncEngine<TSpec>();



        // *****************************************************************
        logger.Debug("Attempting to loop through source specs");
        using( var writer = new StreamWriter(outbound, leaveOpen:true) )
        using( engine.BeginWriteStream(writer) )
        {

            foreach (var spec in sources)
            {

                if( logger.IsTraceEnabled )
                    logger.LogObject(nameof(spec), spec);
                
                engine.WriteNext(spec);

            }

        }


    }

    protected void ProduceStream<TSpec,TTarget>( Stream outbound, IEnumerable<TTarget> sources ) where TSpec : class where TTarget: class
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to build filer helper engine");
        var engine = new FileHelperAsyncEngine<TSpec>();


        // *****************************************************************
        logger.Debug("Attempting to loop through source specs");
        using ( var writer = new StreamWriter(outbound, leaveOpen: true) )
        using( engine.BeginWriteStream(writer) )
        {

            foreach( var source in sources )
            {

                var spec = Mapper.Map<TSpec>(source);

                if( logger.IsTraceEnabled )
                {
                    logger.LogObject(nameof(source), source);
                    logger.LogObject(nameof(spec), spec);
                }

                engine.WriteNext(spec);

            }

        }


    }


}
