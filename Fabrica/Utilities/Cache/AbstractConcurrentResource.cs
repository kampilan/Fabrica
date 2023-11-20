// ReSharper disable UnusedMember.Global
namespace Fabrica.Utilities.Cache;

public abstract class AbstractConcurrentResource<T> : IDisposable
{

    protected abstract Task<IRenewedResource<T>> Renew();


    private ReaderWriterLockSlim ResourceLock { get; } = new ();
    private object RenewLock { get; } = new ();
    private ManualResetEvent RenewComplete { get; } = new (true);

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

        try
        {


            if( Expiration < DateTime.Now )
                RenewComplete.WaitOne(10000);


            ResourceLock.EnterReadLock();


            if( (MustRenewFlag || Renewable < DateTime.Now) && Monitor.TryEnter(RenewLock, 0) )
            {

                RenewComplete.Reset();

                try
                {

                    if (onEnterRenew != null)
                        await onEnterRenew.Invoke();

                    IRenewedResource<T> renewed;
                    try
                    {
                        renewed = await Renew();
                    }
                    catch (Exception)
                    {
                        return _current.Value;
                    }

                    try
                    {

                        ResourceLock.ExitReadLock();
                        ResourceLock.EnterWriteLock();

                        _current = renewed;

                        MustRenewFlag = false;
                        Renewable  = DateTime.Now + _current.TimeToRenew;
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

                    Monitor.Exit(RenewLock);

                }


            }


            return _current.Value;


        }
        finally
        {

            if (ResourceLock.IsReadLockHeld)
                ResourceLock.ExitReadLock();

        }


    }



    public void Dispose()
    {
        ResourceLock?.Dispose();
        RenewComplete?.Dispose();
    }

}