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

using System;
using System.Data.Common;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using JetBrains.Annotations;

namespace Fabrica.Persistence.Connection
{


    public class ConnectionResolver: IConnectionResolver
    {

        public ConnectionResolver( ICorrelation correlation, DbProviderFactory factory, string replicaConnectionStr, string originConnectionStr )
        {

            Correlation          = correlation;
            Factory              = factory;
            ReplicaConnectionStr = replicaConnectionStr;
            OriginConnectionStr  = originConnectionStr;

        }

        private ICorrelation Correlation { get; }

        private string ReplicaConnectionStr { get; }
        private string OriginConnectionStr { get; }


        public DbProviderFactory Factory { get; }


        public DbConnection GetReplicaConnection()
        {

            var logger = Correlation.GetLogger(this);

            try
            {

                logger.EnterMethod();


                logger.Inspect( "Factory type", Factory.GetType().FullName );


                DbConnection conn;
                try
                {

                    // ***********************************************************
                    logger.Debug("Attempting to create connection");
                    conn = Factory.CreateConnection();

                    if (conn == null)
                        throw new Exception("DbConnection is null");

                }
                catch (Exception cause)
                {
                    logger.ErrorFormat(cause, "Could not create connection from factory {0}", Factory.GetType().FullName);
                    throw;
                }


                // ***********************************************************
                try
                {

                    logger.Inspect( "ReplicaConnectionStr", ReplicaConnectionStr );


                    // ***********************************************************
                    logger.Debug("Attempting to set connection string on connection");
                    conn.ConnectionString = ReplicaConnectionStr;



                    // ***********************************************************
                    logger.Debug("Attempting to open connection");
                    conn.Open();



                    // ***********************************************************
                    return conn;


                }
                catch (Exception cause)
                {
                    logger.ErrorFormat(cause, "Could not open connection from driver {0} using {1} ", Factory.GetType().FullName, ReplicaConnectionStr);
                    throw;
                }


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public DbConnection GetOriginConnection()
        {

            var logger = Correlation.GetLogger(this);

            try
            {

                logger.EnterMethod();


                logger.Inspect("Factory type", Factory.GetType().FullName);


                DbConnection conn;
                try
                {

                    // ***********************************************************
                    logger.Debug("Attempting to create connection");
                    conn = Factory.CreateConnection();

                    if( conn == null )
                        throw new Exception( "DbConnection is null" );

                }
                catch (Exception cause)
                {
                    logger.ErrorFormat(cause, "Could not create connection from driver {0}", Factory.GetType().FullName);
                    throw;
                }



                // ***********************************************************
                try
                {


                    logger.Inspect("OriginConnectionStr", OriginConnectionStr);


                    // ***********************************************************
                    logger.Debug("Attempting to set connection string on connection");
                    conn.ConnectionString = OriginConnectionStr;



                    // ***********************************************************
                    logger.Debug("Attempting to open connection");
                    conn.Open();



                    // ***********************************************************
                    return conn;


                }
                catch (Exception cause)
                {
                    logger.ErrorFormat(cause, "Could not open connection from driver {0} using {1} ", Factory.GetType().FullName, OriginConnectionStr);
                    throw;
                }


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public void CloseConnection([NotNull] DbConnection conn)
        {


            var logger = Correlation.GetLogger(this);

            try
            {

                logger.EnterMethod();

                conn.Close();

            }
            finally
            {
                logger.LeaveMethod();
            }


        }



    }


}
