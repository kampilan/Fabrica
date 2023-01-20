
// ReSharper disable UnusedMember.Global

using System.Data;
using AutoMapper;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using FileHelpers;
using RepoDb;


namespace Fabrica.Persistence.Etl;

public class EtlComponent: CorrelatedObject
{


    public EtlComponent(ICorrelation correlation, IMapper mapper, IRuleSet rules) : base(correlation)
    {

        Mapper = mapper;
        Rules = rules;

    }


    private IMapper Mapper { get; }
    private IRuleSet Rules { get; }


    protected EvaluationResults Evaluate(params object[] facts)
    {

        var ec = Rules.GetEvaluationContext();
        ec.AddAllFacts(facts);
        ec.ThrowNoRulesException = false;

        var er = Rules.Evaluate(ec);

        return er;

    }


    public async Task ProcessStream<TSpec>( Stream inbound, Func<TSpec,Task> sink, bool stopOnError = true) where TSpec : class
    {

        if (inbound == null) throw new ArgumentNullException(nameof(inbound));
        if (sink == null) throw new ArgumentNullException(nameof(sink));

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to build processing engine");
        var engine = new FileHelperAsyncEngine<TSpec>();



        // *****************************************************************
        logger.Debug("Attempting to process each inbound record");
        using var reader = new StreamReader(inbound, leaveOpen: true);

        using (engine.BeginReadStream(reader))
        {

            foreach (var spec in engine)
            {

                if (logger.IsTraceEnabled)
                    logger.LogObject(nameof(spec), spec);

                try
                {
                    Evaluate(spec);
                    await sink(spec);
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

    public async Task ProcessStream<TSpec, TTarget>(Stream inbound, Func<TTarget, Task> sink, bool stopOnError = true) where TSpec : class where TTarget : class
    {

        if (inbound == null) throw new ArgumentNullException(nameof(inbound));
        if (sink == null) throw new ArgumentNullException(nameof(sink));

        using var logger = EnterMethod();



        // *****************************************************************
        logger.Debug("Attempting to build processing engine");
        var engine = new FileHelperAsyncEngine<TSpec>();



        // *****************************************************************
        logger.Debug("Attempting to process each inbound record");
        using var reader = new StreamReader(inbound, leaveOpen: true);

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

    public List<TTarget> ProcessStream<TSpec, TTarget>(Stream inbound, bool stopOnError = true) where TSpec : class where TTarget : class
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

    public async Task<int> LoadStream<TSpec,TTarget>( Stream inbound, IDbConnection connection,  bool stopOnError = true, int batchSize=50 ) where TSpec : class where TTarget : class
    {

        if (inbound == null) throw new ArgumentNullException(nameof(inbound));
        if (connection == null) throw new ArgumentNullException(nameof(connection));

        using var logger = EnterMethod();


        logger.Inspect("TSpec Type", typeof(TSpec));
        logger.Inspect("TTarget Type", typeof(TTarget));


        var total = 0;
        var results = new List<TTarget>();


        // *****************************************************************
        logger.Debug("Attempting to build processing engine");
        var engine = new FileHelperAsyncEngine<TSpec>();



        // *****************************************************************
        logger.Debug("Attempting to beginning Transaction");
        using var trx = (await connection.EnsureOpenAsync()).BeginTransaction();


        // *****************************************************************
        logger.Debug("Attempting to process each inbound record");
        using (var reader = new StreamReader(inbound, leaveOpen: true))
        using (engine.BeginReadStream(reader))
        {

            foreach( var spec in engine )
            {

                if( logger.IsTraceEnabled )
                    logger.LogObject(nameof(spec), spec);


                try
                {

                    var target = Mapper.Map<TTarget>(spec);

                    Evaluate(spec, target);

                    results.Add(target);

                    if (results.Count >= batchSize)
                    {
                        logger.DebugFormat("Attempting to persist batch of {0} to database", results.Count);
                        logger.Inspect(nameof(results.Count), results.Count);
                        await Persist();
                    }

                }
                catch (Exception cause)
                {
                    logger.ErrorWithContext(cause, spec, "Caught Exception processing inbound file");
                    if( stopOnError )
                        throw;
                }

            }

        }


        // *****************************************************************
        logger.Debug("Attempting to persist last objects");
        await Persist();


        // *****************************************************************
        logger.Debug("Attempting to commit Transaction");
        trx.Commit();


        async Task Persist()
        {

            if( results.Count == 0 )
                return;

            total =+ await connection.InsertAllAsync( results, batchSize );

            results.Clear();

        }



        // *****************************************************************
        return total;

    }




    public void ProduceStream<TSpec>(Stream outbound, IEnumerable<TSpec> sources) where TSpec : class
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to build filer helper engine");
        var engine = new FileHelperAsyncEngine<TSpec>();



        // *****************************************************************
        logger.Debug("Attempting to loop through source specs");
        using var writer = new StreamWriter(outbound, leaveOpen: true);

        using (engine.BeginWriteStream(writer))
        {

            foreach (var spec in sources)
            {

                if (logger.IsTraceEnabled)
                    logger.LogObject(nameof(spec), spec);

                engine.WriteNext(spec);

            }

        }


    }

    public void ProduceStream<TSpec, TTarget>(Stream outbound, IEnumerable<TTarget> sources) where TSpec : class where TTarget : class
    {

        using var logger = EnterMethod();


        // *****************************************************************
        logger.Debug("Attempting to build filer helper engine");
        var engine = new FileHelperAsyncEngine<TSpec>();


        // *****************************************************************
        logger.Debug("Attempting to loop through source specs");
        using var writer = new StreamWriter(outbound, leaveOpen: true);

        using (engine.BeginWriteStream(writer))
        {

            foreach (var source in sources)
            {

                var spec = Mapper.Map<TSpec>(source);

                if (logger.IsTraceEnabled)
                {
                    logger.LogObject(nameof(source), source);
                    logger.LogObject(nameof(spec), spec);
                }

                engine.WriteNext(spec);

            }

        }


    }



}