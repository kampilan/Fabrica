
using System;
using System.Data.Common;

namespace Fabrica.Persistence.UnitOfWork;

public class InMemoryUnitOfWork: IUnitOfWork
{

    public DbConnection OriginConnection { get; set; }
    public DbTransaction Transaction { get; set; }
    public UnitOfWorkState State { get; set; } = UnitOfWorkState.CanCommit;

    public void CanCommit()
    {

        if (State == UnitOfWorkState.Unknown)
            State = UnitOfWorkState.CanCommit;

    }

    public void MustRollback()
    {
        State = UnitOfWorkState.MustRollback;
    }

    public void Close()
    {
    }
    public void Dispose()
    {
    }


}