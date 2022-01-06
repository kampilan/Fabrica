using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Fabrica.Watch;

// ReSharper disable UnusedMember.Global
namespace Fabrica.One.Plan
{


    public class MemoryPlanSource : AbstractPlanSource, IPlanSource
    {


        public void CopyFrom( Stream source )
        {

            var logger = this.GetLogger();

            Lock.EnterWriteLock();
            try
            {

                logger.EnterMethod();

                var stream = new MemoryStream();

                source.CopyTo(stream);

                stream.Seek(0, SeekOrigin.Begin);

                Source = stream;

                Updated = true;

            }
            finally
            {
                Lock.ExitWriteLock();
                logger.LeaveMethod();
            }


        }


        private ReaderWriterLockSlim Lock { get; } = new ReaderWriterLockSlim();

        private bool Updated { get; set; }
        private MemoryStream Source { get; set; } = new MemoryStream();


        public bool IsEmpty()
        {

            var logger = this.GetLogger();

            Lock.EnterReadLock();
            try
            {

                logger.EnterMethod();


                var empty = Source == null || !Source.CanRead || Source.Length == 0;

                logger.Inspect(nameof(empty), empty);


                return empty;


            }
            finally
            {
                Lock.ExitReadLock();
                logger.LeaveMethod();
            }

        }

        public Task<Stream> GetSource()
        {

            var logger = this.GetLogger();

            Lock.EnterReadLock();
            try
            {

                logger.EnterMethod();

                var stream = new MemoryStream( Source.ToArray() );

                Updated = false;

                return Task.FromResult( (Stream)stream );

            }
            finally
            {
                Lock.ExitReadLock();
                logger.LeaveMethod();
            }
            
        }

        public Task Reload()
        {
            Updated = true;
            return Task.CompletedTask;
        }

        protected override Task<bool> CheckForUpdate()
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                return Task.FromResult(Updated);

            }
            finally
            {
                logger.LeaveMethod();
            }

        }


    }


}
