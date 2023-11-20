// ReSharper disable UnusedMember.Global

using Fabrica.Watch;

namespace Fabrica.Utilities.Cache;

public abstract class AbstractConcurrentResource<T> : IDisposable
{

    protected abstract Task<IRenewedResource<T>> Renew();


    private ReaderWriterLockSlim ResourceLock { get; } = new();

    private SemaphoreSlim RenewLock { get; } = new(1);
    private ManualResetEvent RenewComplete { get; } = new(true);

    protected bool MustRenewFlag { get; set; }
    protected DateTime Renewable { get; private set; }
    protected DateTime Expiration { get; private set; }
    private IRenewedResource<T> _current;

    public async Task Initialize()
    {
        _current = await Renew();

        MustRenewFlag = false;

    }

    public void MustRenew()
    {
        MustRenewFlag = true;
    }

    public async Task<T> GetResource(Func<Task>? onEnterRenew = null, Func<Task>? onExitRenew = null)
    {

        using var logger = this.EnterMethod();


        logger.Inspect(nameof(Renewable), Renewable);
        logger.Inspect(nameof(Expiration), Expiration);


        if (Expiration < DateTime.Now)
            RenewComplete.WaitOne(10000);



        if( (MustRenewFlag || Renewable < DateTime.Now) && RenewLock.Wait(0) )
        {


            // *****************************************************************
            logger.Debug("Attempting to begin Renewal");

            try
            {

                RenewComplete.Reset();

                if (onEnterRenew != null)
                    await onEnterRenew.Invoke();

                IRenewedResource<T> renewed;
                try
                {
                    renewed = await Renew();
                }
                catch (Exception cause)
                {
                    logger.Warning(cause);
                    return _current.Value;
                }

                try
                {

                    ResourceLock.EnterWriteLock();

                    _current = renewed;

                    MustRenewFlag = false;
                    Renewable = DateTime.Now + _current.TimeToRenew;
                    Expiration = DateTime.Now + _current.TimeToLive;

                }
                finally
                {
                    ResourceLock.ExitWriteLock();
                }

                RenewComplete.Set();


            }
            finally
            {

                if (onExitRenew != null)
                    await onExitRenew.Invoke();

                RenewLock.Release();

            }

            // *****************************************************************
            logger.Debug("Renewal completed");


        }


        try
        {
            ResourceLock.EnterReadLock();
            return _current.Value;

        }
        finally
        {
            ResourceLock.ExitReadLock();
        }


    }



    public void Dispose()
    {
        ResourceLock.Dispose();
        RenewComplete.Dispose();
    }

}