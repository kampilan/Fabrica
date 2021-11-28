
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fabrica.Models.Support;
using Fabrica.Utilities.Container;
using JetBrains.Annotations;

namespace Fabrica.Persistence.Patch
{


    public abstract class AbstractPatchRepository : CorrelatedObject, IPatchRepository
    {

        protected AbstractPatchRepository(ICorrelation correlation) : base(correlation)
        {

        }

        protected abstract Task<TEntity> Retrieve<TEntity>(string uid) where TEntity : class, IModel;

        protected abstract Task<TEntity> Create<TEntity>(string uid, IDictionary<string, object> properties) where TEntity : class, IModel;

        protected abstract Task<TEntity> Update<TEntity>( string uid, IDictionary<string, object> properties) where TEntity : class, IModel;

        protected abstract Task Delete<TEntity>( string uid ) where TEntity : class, IModel;

        public abstract Task Save();

        public abstract Task Abort();



        public async Task<object> HandleRetrieve([NotNull] Type reference, [NotNull] string uid )
        {

            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (string.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(uid));

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to get Retrieve method");
            var method = GetType().GetMethod(nameof(Retrieve));
            if (method is null)
                throw new InvalidOperationException($"Could not get {nameof(Retrieve)} method from PatchRepository");



            // *****************************************************************
            logger.Debug("Attempting to call generic retrieve method");
            var task = (Task<object>)method.MakeGenericMethod(reference).Invoke(this, new object[] { uid } );
            if( task is null )
                throw new InvalidOperationException($"{nameof(Retrieve)} did not produce a Task");


            // *****************************************************************
            logger.Debug("Attempting to get retrieved entity");
            var entity = await task;

            logger.LogObject(nameof(entity), entity);



            // *****************************************************************
            return entity;

        }

        public async Task<object> HandleCreate( [NotNull] Type type, [NotNull] string uid, [NotNull] IDictionary<string, object> properties )
        {

            if (type == null) throw new ArgumentNullException(nameof(type));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            if (string.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(uid));

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to get Create method");
            var method = GetType().GetMethod(nameof(Create));
            if (method is null)
                throw new InvalidOperationException($"Could not get {nameof(Create)} method from PatchRepository");



            // *****************************************************************
            logger.Debug("Attempting to call generic Create method");
            var task = (Task<object>)method.MakeGenericMethod(type).Invoke(this, new object[] { uid });
            if (task is null)
                throw new InvalidOperationException($"{nameof(Create)} did not produce a Task");


            // *****************************************************************
            logger.Debug("Attempting to get Created entity");
            var entity = await task;

            logger.LogObject(nameof(entity), entity);




            // *****************************************************************
            return entity;






        }

        public async Task<object> HandleUpdate( Type type, string uid, IDictionary<string, object> properties )
        {

            if (type == null) throw new ArgumentNullException(nameof(type));
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            if (string.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(uid));

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to get Update method");
            var method = GetType().GetMethod(nameof(Create));
            if (method is null)
                throw new InvalidOperationException($"Could not get {nameof(Update)} method from PatchRepository");



            // *****************************************************************
            logger.Debug("Attempting to call generic Update method");
            var task = (Task<object>)method.MakeGenericMethod(type).Invoke(this, new object[] { uid });
            if (task is null)
                throw new InvalidOperationException($"{nameof(Update)} did not produce a Task");


            // *****************************************************************
            logger.Debug("Attempting to get Update entity");
            var entity = await task;

            logger.LogObject(nameof(entity), entity);



            // *****************************************************************
            return entity;


        }

        public async Task HandleDelete([NotNull] Type type, [NotNull] string uid)
        {

            if (type == null) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(uid));

            using var logger = EnterMethod();


            // *****************************************************************
            logger.Debug("Attempting to get Delete method");
            var method = GetType().GetMethod(nameof(Create));
            if (method is null)
                throw new InvalidOperationException($"Could not get {nameof(Delete)} method from PatchRepository");



            // *****************************************************************
            logger.Debug("Attempting to call generic Delete method");
            var task = (Task)method?.MakeGenericMethod(type).Invoke(this, new object[] { uid });
            if (task is null)
                throw new InvalidOperationException($"{nameof(Delete)} did not produce a Task");

            await task;

        }



    }


}
