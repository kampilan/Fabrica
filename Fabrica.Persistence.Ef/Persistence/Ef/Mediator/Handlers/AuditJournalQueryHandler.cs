using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Mediator;
using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Persistence.Mediator;
using Fabrica.Persistence.Thin;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Sql;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Persistence.Ef.Mediator.Handlers
{

    
    public class AuditJournalQueryHandler: AbstractRequestHandler<AuditJournalQueryRequest,MemoryStream>
    {

        private const string QueryJournal = "select * from AuditJournals as oaj where oaj.UnitOfWorkUid in (select distinct iaj.UnitOfWorkUid from AuditJournals as iaj where iaj.Entity={0} and iaj.EntityUid = {1}) and oaj.TypeCode <> 'UnmodifiedRoot'";

        private static readonly ISet<string> Exclusions = new HashSet<string> {"Id"};


        public AuditJournalQueryHandler( ICorrelation correlation, ReplicaDbContext context) : base( correlation )
        {
            Context = context;
        }        
       
        
        private ReplicaDbContext Context { get; }

        protected override async Task<MemoryStream> Perform( CancellationToken cancellationToken=default )
        {

            using var logger = EnterMethod();

            var strm = new MemoryStream();


            // *****************************************************************
            logger.Debug("Attempting to establish Db connection");
            await using var cn = Context.Database.GetDbConnection();
            if( cn.State == ConnectionState.Closed )
                await cn.OpenAsync(cancellationToken);



            // *****************************************************************
            logger.Debug("Attempting to setup DbCommand");
            await using var cmd = cn.CreateCommand();
            cmd.FromSqlRaw(QueryJournal, Request.Entity, Request.EntityUid);



            // *****************************************************************
            logger.Debug("Attempting to serializer reader to json");
            await using (var writer = new StreamWriter(strm, leaveOpen:true) )
            await using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                await reader.ToJson(writer,Exclusions);
            }

            strm.Seek(0, SeekOrigin.Begin);

            return strm;


        }

    }

}
