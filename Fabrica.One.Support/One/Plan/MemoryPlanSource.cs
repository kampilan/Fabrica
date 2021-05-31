using System.IO;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
namespace Fabrica.One.Plan
{


    public class MemoryPlanSource: IPlanSource
    {


        public void CopyFrom( Stream source )
        {

            Lock.EnterWriteLock();
            try
            {

                var stream = new MemoryStream();

                source.CopyTo(stream);

                stream.Seek(0, SeekOrigin.Begin);

                Source = stream;

                Updated = true;

            }
            finally
            {
                Lock.ExitWriteLock();
            }

        }


        private ReaderWriterLockSlim Lock { get;  } = new ReaderWriterLockSlim();

        private bool Updated { get; set; }
        private MemoryStream Source { get; set; } = new MemoryStream();


        public Task<Stream> GetSource()
        {

            Lock.EnterReadLock();
            try
            {

                var stream = new MemoryStream( Source.ToArray() );

                Updated = false;

                return Task.FromResult( (Stream)stream );

            }
            finally
            {
                Lock.ExitReadLock();
            }
            
        }


        public Task<bool> HasUpdatedPlan() => Task.FromResult(Updated);


    }


}
