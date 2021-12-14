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
using System.Threading.Tasks;
using Fabrica.Utilities.Container;
using Fabrica.Watch;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Fabrica.Api.Support.Middleware
{

    public class DebugMiddleware
    {

        public DebugMiddleware(RequestDelegate next)
        {
            Next = next;
        }
        
        private RequestDelegate Next { get; }


        public Task Invoke( [NotNull] HttpContext context, [NotNull] ICorrelation correlation )
        {
            
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (correlation == null) throw new ArgumentNullException(nameof(correlation));

            using( var logger = correlation.GetLogger(this) )
            {

                var debug = false;

                // *****************************************************************
                logger.Debug("Attempting to check for Fabrica-Watch-Debug header");
                if (context.Request.Headers.ContainsKey("X-Fabrica-Watch-Debug"))
                {

                    logger.Debug("Fabrica-Watch-Debug IS present");
                    var candidate = context.Request.Headers["X-Fabrica-Watch-Debug"];

                    logger.Inspect(nameof(candidate), candidate);


                    logger.Debug("Attempting to check candidate is a valid int");
                    if (int.TryParse(candidate, out var debugFlag))
                        debug = debugFlag != 0;
                    else
                        logger.Debug("Not a valid value");

                }
                else
                {
                    logger.Debug("X-Fabrica-Watch-Debug IS NOT present");
                }


                ((Correlation)correlation).Debug = debug;


            }


            return Next(context);


        }


    }

}
