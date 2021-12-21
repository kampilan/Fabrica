/*
The MIT License (MIT)

Copyright (c) 2017 The Kampilan Group Inc.

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
using Autofac;
using Fabrica.Utilities.Container;
using SmartFormat;

namespace Fabrica.Persistence.Connection
{


    public static class AutofacExtensions
    {


        public static ContainerBuilder AddSingleTenantResolver( this ContainerBuilder builder, DbProviderFactory factory, string replicaConnectionStr, string originConnectionStr )
        {

            builder.Register(c =>
            {

                var correlation = c.Resolve<ICorrelation>();

                var comp = new ConnectionResolver( correlation, factory, replicaConnectionStr, originConnectionStr );

                return comp;

            })
                .As<IConnectionResolver>()
                .InstancePerLifetimeScope();


            return builder;

        }

        public static ContainerBuilder AddSingleTenantResolver<TSecrets>(this ContainerBuilder builder, DbProviderFactory factory, Action<TSecrets,DbCredentials> credBuilder, string replicaConnectionTemplate, string originConnectionTemplate) where TSecrets: class
        {

            builder.Register(c =>
                {

                    var correlation = c.Resolve<ICorrelation>();
                    var secrets     = c.Resolve<TSecrets>();

                    var creds = new DbCredentials();
                    credBuilder(secrets, creds);

                    var replica = Smart.Format( replicaConnectionTemplate, creds );
                    var origin  = Smart.Format( originConnectionTemplate, creds );

                    var comp = new ConnectionResolver(correlation, factory, replica, origin );

                    return comp;

                })
                .As<IConnectionResolver>()
                .InstancePerLifetimeScope();


            return builder;

        }




        public static ContainerBuilder AddSingleTenantResolver( this ContainerBuilder builder, DbProviderFactory factory, ISingleTenantPersistenceModule module )
        {

            builder.Register(c =>
            {

                var correlation = c.Resolve<ICorrelation>();

                var comp = new ConnectionResolver( correlation, factory, module.ReplicaConnectionStr, module.MasterConnectionStr );

                return comp;

            })
                .As<IConnectionResolver>()
                .InstancePerLifetimeScope();


            return builder;

        }


    }


}
