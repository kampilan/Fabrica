/*
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

using System.Data;
using System.Data.Common;
using Fabrica.Persistence.Connection;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using JetBrains.Annotations;

namespace Fabrica.Persistence.UnitOfWork;

public class UnitOfWork: IUnitOfWork
{


    public UnitOfWork( ICorrelation context, IConnectionResolver resolver )
    {

        Correlation = context;

        Resolver = resolver;

    }

    private ICorrelation Correlation { get; }


    private IConnectionResolver Resolver { get; }

    private DbConnection? Connection { get; set; }

    [NotNull]
    public DbConnection OriginConnection
    {
        get
        {
            if( Connection != null )
                return Connection;

            Connection = Resolver.GetOriginConnection();

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            Transaction = Connection.BeginTransaction();

            return Connection;

        }
    }


    public DbTransaction Transaction { get; private set; } = null!;
    public UnitOfWorkState State { get; private set; } = UnitOfWorkState.CanCommit;

    public void CanCommit()
    {

        if( State == UnitOfWorkState.Unknown )
            State = UnitOfWorkState.CanCommit;

    }

    public void MustRollback()
    {
        State = UnitOfWorkState.MustRollback;
    }


    private bool IsClosed { get; set; }

    public void Close()
    {

        var logger = Correlation.GetLogger(this);

        try
        {

            logger.EnterMethod();

            logger.Inspect( nameof(IsClosed), IsClosed );
            logger.Inspect( nameof(State), State );


            if( IsClosed )
                return;


            // *****************************************************************
            if ( Transaction != null )
            {
                if (State == UnitOfWorkState.CanCommit)
                    Transaction.Commit();
                else
                    Transaction.Rollback();

                Transaction.Dispose();
                Transaction = null;

            }



            // *****************************************************************
            if (Connection != null)
            {
                if (Connection.State == ConnectionState.Open)
                    Connection.Close();

                Connection = null;

            }



            // *****************************************************************
            IsClosed = true;


        }
        finally
        {
            logger.LeaveMethod();
        }


    }


    public void Dispose()
    {
        Close();
    }


}