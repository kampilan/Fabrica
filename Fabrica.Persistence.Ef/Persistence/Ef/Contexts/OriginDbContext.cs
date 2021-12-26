﻿/*
The MIT License (MIT)

Copyright (c) 2021 The Kampilan Group Inc.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Identity;
using Fabrica.Models;
using Fabrica.Models.Support;
using Fabrica.Persistence.Audit;
using Fabrica.Rules;
using Fabrica.Rules.Exceptions;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Threading;
using Fabrica.Watch;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace Fabrica.Persistence.Ef.Contexts
{


    public class OriginDbContext : BaseDbContext
    {


        public OriginDbContext(ICorrelation correlation, IRuleSet rules, [NotNull] DbContextOptions options, ILoggerFactory factory = null) : base(correlation, options, factory)
        {

            Rules = rules;

        }


        public bool EvaluatateEntities { get; set; } = true;

        protected IRuleSet Rules { get; }


        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {


            var logger = Correlation.GetLogger(this);

            try
            {

                logger.EnterMethod();


                var result = AsyncPump.Run(() => SaveChangesAsync(acceptAllChangesOnSuccess));

                return result;

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default )
        {


            var logger = Correlation.GetLogger(this);

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to create an EvaluationContext");
                var context = Rules.GetEvaluationContext();



                // *****************************************************************
                logger.Debug("Attempting to inspect each entity in this DbContext");
                foreach (var entry in ChangeTracker.Entries())
                {


                    logger.Inspect(nameof(entry.Entity), entry.Entity.GetType().FullName);



                    if( entry.Entity is IMutableModel mm && (entry.State is EntityState.Added or EntityState.Modified) )
                    {


                        logger.DebugFormat("Attempting to call lifecycle hooks on mutable entity for State {0}", entry.State);

                        if (entry.State == EntityState.Added)
                            mm.OnCreate();

                        mm.OnModification();


                    }




                    // *****************************************************************
                    if( EvaluatateEntities && (entry.State is EntityState.Added or EntityState.Modified) )
                        context.AddFacts(entry.Entity);



                }



                // *****************************************************************
                logger.Debug("Attempting to evaluate added and modified entities");
                context.ThrowNoRulesException = false;
                context.ThrowValidationException = false;

                var er = Rules.Evaluate(context);

                logger.LogObject(nameof(er), er);

                if( er.HasViolations )
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
            finally
            {
                logger.LeaveMethod();
            }


        }



        #region Journaling


        public IModel Root { get; set; }



        public DbSet<AuditJournalModel> AuditJournals { get; set; }


        public bool PerformAuditing { get; set; } = true;


        [NotNull]
        protected virtual AuditJournalModel CreateAuditJournal(DateTime journalTime, AuditJournalType type, [NotNull] IModel entity, [CanBeNull] PropertyEntry prop = null)
        {

            var ident = Correlation.ToIdentity() ?? new ClaimsIdentity();

            var aj = new AuditJournalModel
            {
                TypeCode = type.ToString(),
                UnitOfWorkUid = Correlation.Uid,
                SubjectUid = ident.GetSubject(),
                SubjectDescription = ident.GetName(),
                Occurred = journalTime,
                Entity = entity.GetType().FullName,
                EntityUid = entity.Uid,
                EntityDescription = entity.ToString()
            };


            if (prop != null)
            {

                aj.PropertyName = prop.Metadata.Name;

                var prev = prop.OriginalValue?.ToString() ?? "";
                if (prev.Length > 255)
                    prev = prev.Substring(0, 255);

                aj.PreviousValue = prev;

                var curr = prop.CurrentValue?.ToString() ?? "";
                if (curr.Length > 255)
                    curr = curr.Substring(0, 255);

                aj.CurrentValue = curr;

            }


            return aj;

        }



        protected virtual IList<AuditJournalModel> PerformJournaling()
        {

            var logger = GetLogger();

            try
            {

                logger.EnterMethod();

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

                    if (audit == null || (!audit.Read && !audit.Write))
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
            finally
            {
                logger.LeaveMethod();
            }


        }



        protected virtual void PerformDetailJournaling([NotNull] EntityEntry entry, IList<AuditJournalModel> journals, DateTime journalTime)
        {


            var logger = GetLogger();

            try
            {

                logger.EnterMethod();


                if (!(entry.Entity is IModel entity))
                    return;



                foreach (var prop in entry.Properties)
                {

                    if (!prop.IsModified)
                        continue;

                    var aj = CreateAuditJournal(journalTime, AuditJournalType.Detail, entity, prop);

                    journals.Add(aj);

                }


            }
            finally
            {
                logger.LeaveMethod();
            }


        }



        #endregion







    }





}