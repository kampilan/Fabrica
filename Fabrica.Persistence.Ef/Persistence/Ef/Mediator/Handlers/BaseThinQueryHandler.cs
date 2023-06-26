// ReSharper disable UnusedMember.Global

using System.Data;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Models.Support;
using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.Thin;
using Fabrica.Rql.Builder;
using Fabrica.Rql.Serialization;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Sql;
using Fabrica.Watch;
using Humanizer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Ef.Mediator.Handlers
{

    
    public abstract class BaseThinQueryHandler<TRequest,TResponse> : BaseHandler<TRequest, MemoryStream> where TRequest : class, IRequest<Response<MemoryStream>>, IQueryEntityRequest<TResponse> where TResponse : class, IModel
    {


        protected BaseThinQueryHandler( ICorrelation correlation, IModelMetaService meta, IRuleSet rules, ReplicaDbContext context ) : base( correlation )
        {

            Meta    = meta;
            Rules   = rules;
            Context = context;


            var mm = meta.GetMetaFromType(typeof(TResponse));

            Projection = new HashSet<string>(mm.Projection);
            Projection.ExceptWith(mm.Exclusions);

            TableName = typeof(TResponse).Name.Pluralize();
            Filter    = RqlFilterBuilder<TResponse>.All();

        }


        protected IModelMetaService Meta { get; }
        protected IRuleSet Rules { get; }
        protected ReplicaDbContext Context { get; }


        protected ISet<string> Projection { get; set; }
        protected string TableName { get; set; }
        protected RqlFilterBuilder<TResponse> Filter { get; set; }


        protected virtual void PrepareQuery()
        {

        }


        protected override async Task<MemoryStream> Perform(CancellationToken cancellationToken = default)
        {

            using var logger = EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to get first filter");
            var filter = Request.Filters.FirstOrDefault();

            // ReSharper disable once JoinNullCheckWithUsage
            if( filter is null)
                throw new PredicateException("At least 1 filter must be defined to use Thin query");

            Filter = (RqlFilterBuilder<TResponse>) filter;



            // *****************************************************************
            logger.Debug("Attempting to delegate to Prepare");
            PrepareQuery();



            // *****************************************************************
            logger.Debug("Attempting to evaluate filters");
            var ec = Rules.GetEvaluationContext();
            ec.ThrowNoRulesException = false;

            ec.AddFacts(Filter);

            var er = Rules.Evaluate(ec);

            logger.LogObject(nameof(er), er);



            // *****************************************************************
            logger.Debug("Attempting to establish Db connection");
            await using var cn = Context.Database.GetDbConnection();
            if (cn.State == ConnectionState.Closed)
                await cn.OpenAsync(cancellationToken);



            // *****************************************************************
            logger.Debug("Attempting to build SQL query from RQL filter");
            var (sql, parameters) = Filter.ToSqlQuery( TableName, Projection );

            logger.Inspect(nameof(sql),sql);




            // *****************************************************************
            logger.Debug("Attempting to setup DbCommand");
            await using var cmd = cn.CreateCommand();
            cmd.FromSqlRaw( sql, parameters );



            // *****************************************************************
            logger.Debug("Attempting to serializer reader to json");
            // ReSharper disable once IdentifierTypo
            var strm = new MemoryStream();
            await using (var writer = new StreamWriter(strm, leaveOpen: true))
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                await reader.ToJson( writer );
            }

            strm.Seek(0, SeekOrigin.Begin);



            // *****************************************************************
            return strm;


        }






    }




}
