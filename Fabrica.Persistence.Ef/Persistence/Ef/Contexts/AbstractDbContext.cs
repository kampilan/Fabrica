using Fabrica.Identity;
using Fabrica.Models.Support;
using Fabrica.Models;
using Fabrica.Persistence.Audit;
using Fabrica.Persistence.UnitOfWork;
using Fabrica.Rules.Exceptions;
using Fabrica.Rules;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.CompilerServices;
using Fabrica.Rules.Factory;
using ILogger = Fabrica.Watch.ILogger;


namespace Fabrica.Persistence.Ef.Contexts;

public abstract class AbstractDbContext: DbContext
{


    protected AbstractDbContext( OriginDbContextOptionsBuilder builder ) : base(builder.Options)
    {

        Correlation = builder.Correlation;
        Uow         = builder.Uow;
        Rules       = builder.Rules;
        Factory     = builder.LoggerFactory;

        IsReadonly       = false;
        EvaluateEntities = true;
        PerformAuditing  = true;

    }


    protected AbstractDbContext( ReplicaDbContextOptionsBuilder builder ) : base(builder.Options)
    {

        Correlation = builder.Correlation;
        Rules       = new RuleSet();
        Factory     = builder.LoggerFactory;

        IsReadonly       = true;
        EvaluateEntities = false;
        PerformAuditing  = false;

    }


    protected bool IsReadonly { get; }
    protected bool EvaluateEntities { get; }
    protected bool PerformAuditing { get; }



    protected ICorrelation Correlation { get; }

    private ILoggerFactory Factory { get; }

    protected IUnitOfWork? Uow { get; }
    protected IRuleSet Rules { get; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseLoggerFactory(Factory);

    }


    protected ILogger GetLogger()
    {

        var logger = Correlation.GetLogger(this);

        return logger;

    }

    protected ILogger EnterMethod([CallerMemberName] string name = "")
    {

        var logger = Correlation.EnterMethod(GetType(), name);

        return logger;

    }



    protected async Task EnlistUnitOfWork()
    {

        using var logger = EnterMethod();


        // *****************************************************************
        if (Uow is not null)
        {
            logger.Debug("Attempting to enlisting Transaction from Uow");
            await Database.UseTransactionAsync(Uow.Transaction);
        }

    }


    private void ReadonlyCheck()
    {
        if( IsReadonly )
            throw new InvalidOperationException("This is a read only DbContext. No changes are allowed");
    }


    public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
    {
        ReadonlyCheck();
        return base.Add(entity);
    }

    public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = new ())
    {
        ReadonlyCheck();
        return base.AddAsync(entity, cancellationToken);
    }

    public override EntityEntry Add(object entity)
    {
        ReadonlyCheck();
        return base.Add(entity);
    }

    public override ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = new CancellationToken())
    {
        ReadonlyCheck();
        return base.AddAsync(entity, cancellationToken);
    }


    public override void AddRange(params object[] entities)
    {
        ReadonlyCheck();
        base.AddRange(entities);
    }

    public override void AddRange(IEnumerable<object> entities)
    {
        ReadonlyCheck();
        base.AddRange(entities);
    }

    public override Task AddRangeAsync(params object[] entities)
    {
        ReadonlyCheck();
        return base.AddRangeAsync(entities);
    }

    public override Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = new CancellationToken())
    {
        ReadonlyCheck();
        return base.AddRangeAsync(entities, cancellationToken);
    }

    public override EntityEntry<TEntity> Attach<TEntity>(TEntity entity)
    {
        ReadonlyCheck();
        return base.Attach(entity);
    }

    public override EntityEntry Attach(object entity)
    {
        ReadonlyCheck();
        return base.Attach(entity);
    }

    public override void AttachRange(params object[] entities)
    {
        ReadonlyCheck();
        base.AttachRange(entities);
    }

    public override void AttachRange(IEnumerable<object> entities)
    {
        ReadonlyCheck();
        base.AttachRange(entities);
    }

    public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
    {
        ReadonlyCheck();
        return base.Remove(entity);
    }

    public override void RemoveRange(params object[] entities)
    {
        ReadonlyCheck();
        base.RemoveRange(entities);
    }

    public override EntityEntry Remove(object entity)
    {
        ReadonlyCheck();
        return base.Remove(entity);
    }

    public override void RemoveRange(IEnumerable<object> entities)
    {
        ReadonlyCheck();
        base.RemoveRange(entities);
    }

    public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
    {
        ReadonlyCheck();
        return base.Update(entity);
    }

    public override EntityEntry Update(object entity)
    {
        ReadonlyCheck();
        return base.Update(entity);
    }

    public override void UpdateRange(params object[] entities)
    {
        ReadonlyCheck();
        base.UpdateRange(entities);
    }

    public override void UpdateRange(IEnumerable<object> entities)
    {
        ReadonlyCheck();
        base.UpdateRange(entities);
    }


    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {

        using var logger = EnterMethod();

        if( IsReadonly )
            throw new InvalidOperationException("This is a read only DbContext. No changes are allowed");

        var result = Task.Run(async () => await SaveChangesAsync(acceptAllChangesOnSuccess)).ConfigureAwait(false).GetAwaiter().GetResult();

        return result;

    }


    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {

        using var logger = EnterMethod();


        if( IsReadonly )
            throw new InvalidOperationException("Can not save changes on this DbContext as it is Readonly");



        // *****************************************************************
        logger.Debug("Attempting to create an EvaluationContext");
        var context = Rules.GetEvaluationContext();



        // *****************************************************************
        logger.Debug("Attempting to inspect each entity in this DbContext");
        foreach (var entry in ChangeTracker.Entries())
        {


            logger.Inspect(nameof(entry.Entity), entry.Entity.GetType().FullName);



            if (entry is { Entity: IMutableModel mm, State: EntityState.Added or EntityState.Modified })
            {


                logger.Debug("Attempting to call lifecycle hooks on mutable entity for State {0}", entry.State);

                if (entry.State == EntityState.Added)
                    mm.OnCreate();

                mm.OnModification();


            }




            // *****************************************************************
            if (EvaluateEntities && (entry.State is EntityState.Added or EntityState.Modified))
                context.AddFacts(entry.Entity);



        }



        // *****************************************************************
        logger.Debug("Attempting to evaluate added and modified entities");
        context.ThrowNoRulesException = false;
        context.ThrowValidationException = false;

        var er = Rules.Evaluate(context);

        logger.LogObject(nameof(er), er);

        if (er.HasViolations)
            throw new ViolationsExistException(er);



        // *****************************************************************
        logger.Debug("Attempting to perform auditing");
        var list = PerformJournaling();



        // *****************************************************************
        logger.Debug("Attempting to save changes");
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);



        logger.Inspect("Audit journal count", list.Count);
        if (list.Count > 0)
        {


            // *****************************************************************
            logger.Debug("Attempting to add journal entities to context");
            await AuditJournals.AddRangeAsync(list, cancellationToken);



            // *****************************************************************
            logger.Debug("Attempting to save changes again to save audit journals");
            await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);


        }



        // *****************************************************************
        return result;




    }




    public IModel? Root { get; set; }


    public DbSet<AuditJournalModel> AuditJournals { get; set; } = null!;


    protected virtual AuditJournalModel CreateAuditJournal(DateTime journalTime, AuditJournalType type, IModel entity, PropertyEntry? prop = null)
    {

        var ident = Correlation.ToIdentity();

        var aj = new AuditJournalModel
        {
            TypeCode = type.ToString(),
            UnitOfWorkUid = Correlation.Uid,
            SubjectUid = ident.GetSubject(),
            SubjectDescription = ident.GetName(),
            Occurred = journalTime,
            Entity = entity.GetType().FullName ?? "",
            EntityUid = entity.Uid,
            EntityDescription = entity.ToString() ?? ""
        };


        if (prop != null)
        {

            aj.PropertyName = prop.Metadata.Name;

            var prev = prop.OriginalValue?.ToString() ?? "";
            if (prev.Length > 255)
                prev = prev[..255];

            aj.PreviousValue = prev;

            var curr = prop.CurrentValue?.ToString() ?? "";
            if (curr.Length > 255)
                curr = curr[..255];

            aj.CurrentValue = curr;

        }


        return aj;

    }



    protected virtual IList<AuditJournalModel> PerformJournaling()
    {

        using var logger = EnterMethod();



        var journalTime = DateTime.Now;
        var journals = new List<AuditJournalModel>();



        // *****************************************************************
        logger.Inspect(nameof(PerformAuditing), PerformAuditing);

        if (!PerformAuditing)
            return journals;



        // *****************************************************************
        logger.Debug("Attempting to check if there are pending changes to audit");
        var hasChanges = ChangeTracker.HasChanges();

        logger.Inspect(nameof(hasChanges), hasChanges);

        if (!hasChanges)
            return journals;

        if (Root != null)
        {

            logger.Debug("Attempting to create unmodified root journal entry");
            var aj = CreateAuditJournal(journalTime, AuditJournalType.UnmodifiedRoot, Root);

            journals.Add(aj);
        }


        // *****************************************************************
        logger.Debug("Attempting to inspect each entity in this DbContext");
        foreach (var entry in ChangeTracker.Entries())
        {

            logger.Inspect("EntityType", entry.Entity.GetType().FullName);
            logger.Inspect("State", entry.State);


            // *****************************************************************
            logger.Debug("Attempting to check if entity is Model");
            if (entry.Entity is not IModel entity)
                continue;



            // *****************************************************************
            logger.Debug("Attempting to get AuditAttribute");
            var audit = entry.Entity.GetType().GetCustomAttribute<AuditAttribute>(true);

            logger.LogObject(nameof(audit), audit);

            if (audit == null || audit is { Read: false, Write: false })
                continue;



            // *****************************************************************                    
            if (audit.Read)
            {

                logger.Debug("Attempting to create read journal entry");
                var aj = CreateAuditJournal(journalTime, AuditJournalType.Read, entity);
                journals.Add(aj);

            }



            // *****************************************************************
            if (entry.State == EntityState.Added && audit.Write)
            {

                logger.Debug("Attempting to create insert journal entry");
                var aj = CreateAuditJournal(journalTime, AuditJournalType.Created, entity);

                journals.Add(aj);

                if (audit.Detailed)
                    PerformDetailJournaling(entry, journals, journalTime);

            }



            // *****************************************************************
            if (entry.State == EntityState.Modified && audit.Write)
            {

                logger.Debug("Attempting to create update journal entry");
                var aj = CreateAuditJournal(journalTime, AuditJournalType.Updated, entity);

                journals.Add(aj);

                if (audit.Detailed)
                    PerformDetailJournaling(entry, journals, journalTime);

            }



            // *****************************************************************
            if (entry.State == EntityState.Deleted && audit.Write)
            {

                logger.Debug("Attempting to create delete journal entry");
                var aj = CreateAuditJournal(journalTime, AuditJournalType.Deleted, entity);

                journals.Add(aj);

            }



            // *****************************************************************
            if (entry.State == EntityState.Unchanged && audit.Write && entity is IRootModel && Root == null)
            {

                logger.Debug("Attempting to create unmodified root journal entry");
                var aj = CreateAuditJournal(journalTime, AuditJournalType.UnmodifiedRoot, entity);

                journals.Add(aj);

            }


        }



        // *****************************************************************        
        logger.Inspect("Journal Count", journals.Count);
        return journals;



    }



    protected virtual void PerformDetailJournaling(EntityEntry entry, IList<AuditJournalModel> journals, DateTime journalTime)
    {

        using var logger = EnterMethod();



        if (entry.Entity is not IModel entity)
            return;



        foreach (var prop in entry.Properties)
        {

            if (!prop.IsModified)
                continue;

            var aj = CreateAuditJournal(journalTime, AuditJournalType.Detail, entity, prop);

            journals.Add(aj);

        }


    }

















}