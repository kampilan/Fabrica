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
using System.Runtime.CompilerServices;
using Fabrica.Utilities.Container;
using JetBrains.Annotations;

namespace Fabrica.Watch
{

    public static class WatchExtensions
    {


        public static ILogger GetLogger( [NotNull] this ICorrelation correlation, [NotNull] string category )
        {

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(category));



            // ******************************************************
            var request = new LoggerRequest
            {
                Debug         = correlation.Debug,
                Tenant        = correlation.Tenant,
                Subject       = correlation.Caller?.Identity?.Name??"",
                Category      = category,
                CorrelationId = correlation.Uid
            };

            request.FilterKeys.Add(("Subject", correlation.Caller?.Identity?.Name??""));
            request.FilterKeys.Add(("Tenant", correlation.Tenant));



            // ******************************************************
            var logger = WatchFactoryLocator.Factory.GetLogger(request);



            // ******************************************************
            return logger;


        }


        public static ILogger GetLogger<T>( [NotNull] this ICorrelation correlation )
        {

            var category = typeof(T).FullName??"Unknown";

            return GetLogger( correlation, category );


        }

        public static ILogger GetLogger( [NotNull] this ICorrelation correlation, [NotNull] Type type )
        {

            if (type == null) throw new ArgumentNullException(nameof(type));

            var category = type.FullName??"Unknown";

            return GetLogger( correlation, category );

        }


        public static ILogger GetLogger( [NotNull] this ICorrelation correlation, [NotNull] object target )
        {

            if (target == null) throw new ArgumentNullException(nameof(target));

            var category = target.GetType().FullName??"Unknown";

            return GetLogger( correlation, category );

        }


        public static ILogger EnterMethod([NotNull] this ICorrelation correlation, [NotNull] string category, [CallerMemberName] string name="" )
        {

            var logger = GetLogger(correlation, category );
            logger.EnterMethod(name);

            return logger;

        }

        public static ILogger EnterMethod<T>([NotNull] this ICorrelation correlation, [CallerMemberName] string name="")
        {
           
            var logger = GetLogger<T>(correlation);
            logger.EnterMethod( name );

            return logger;

        }

        public static ILogger EnterMethod([NotNull] this ICorrelation correlation, [NotNull] Type type, [CallerMemberName] string name="" )
        {

            var logger = GetLogger( correlation, type );
            logger.EnterMethod(name);

            return logger;

        }


        public static ILogger GetLogger( [NotNull] this object target )
        {
            var logger = WatchFactoryLocator.Factory.GetLogger( target.GetType() );
            return logger;
        }

        public static ILogger EnterMethod( [NotNull] this object target, [CallerMemberName] string name="" )
        {
            var logger = WatchFactoryLocator.Factory.GetLogger(target.GetType());
            logger.EnterMethod( name );
            return logger;
        }


    }


}
