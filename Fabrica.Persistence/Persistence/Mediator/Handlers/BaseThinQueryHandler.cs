using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Exceptions;
using Fabrica.Mediator;
using Fabrica.Mediator.Requests;
using Fabrica.Models.Support;
using Fabrica.Persistence.Contexts;
using Fabrica.Persistence.Thin;
using Fabrica.Rql.Serialization;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Sql;
using Fabrica.Utilities.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Mediator.Handlers
{

    
    public abstract class BaseThinQueryHandler<TRequest,TResponse> : BaseHandler<TRequest, MemoryStream> where TRequest : class, IRequest<Response<MemoryStream>>, IQueryRequest<TResponse> where TResponse : class, IModel
    {


        protected BaseThinQueryHandler( ICorrelation correlation, IModelMetaService meta, IRuleSet rules, ReplicaDbContext context ) : base( correlation )
        {

            Meta    = meta;
            Rules   = rules;
            Context = context;

        }


        protected IModelMetaService Meta { get; }
        protected IRuleSet Rules { get; }
        protected ReplicaDbContext Context { get; }


        protected virtual (string sql, object[] parameters) GetSqlTemplate()
        {

            using var logger = EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to get first filter");
            var filter = Request.Filters.FirstOrDefault();
            if (filter == null)
                throw new PredicateException("At least 1 filter must be defined to use Thin query");



            // *****************************************************************
            logger.Debug("Attempting to fetch ModelMeta");
            var mm = Meta.GetMetaFromType(typeof(TResponse));



            // *****************************************************************
            logger.Debug("Attempting to build SQL query from RQL filter");
            var query = filter.ToSqlQuery( typeof(TResponse).Name.Pluralize(), mm.Projection );



            // *****************************************************************
            return query;

        }

        protected virtual ISet<string> GetExclusions()
        {

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to fetch ModelMeta");
            var mm = Meta.GetMetaFromType( typeof(TResponse) );



            // *****************************************************************
            return mm.Exclusions;

        }


        protected override async Task<MemoryStream> Perform(CancellationToken cancellationToken = default)
        {

            using var logger = EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to evaluate filters");
            var ec = Rules.GetEvaluationContext();
            ec.ThrowNoRulesException = false;

            ec.AddAllFacts(Request.Filters);

            var er = Rules.Evaluate(ec);

            logger.LogObject(nameof(er), er);



            // *****************************************************************
            logger.Debug("Attempting to establish Db connection");
            await using var cn = Context.Database.GetDbConnection();
            if (cn.State == ConnectionState.Closed)
                await cn.OpenAsync(cancellationToken);



            // *****************************************************************
            logger.Debug("Attempting to get sql template");
            var (sql, parameters) = GetSqlTemplate();

            logger.Inspect(nameof(sql),sql);



            // *****************************************************************
            logger.Debug("Attempting to get exclusions");
            var exclusions = GetExclusions();

            logger.LogObject(nameof(exclusions), exclusions);



            // *****************************************************************
            logger.Debug("Attempting to setup DbCommand");
            await using var cmd = cn.CreateCommand();
            cmd.FromSqlRaw( sql, parameters );



            // *****************************************************************
            logger.Debug("Attempting to serializer reader to json");
            var strm = new MemoryStream();
            await using (var writer = new StreamWriter(strm, leaveOpen: true))
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                await reader.ToJson( writer, exclusions );
            }

            strm.Seek(0, SeekOrigin.Begin);



            // *****************************************************************
            return strm;


        }






    }




}
