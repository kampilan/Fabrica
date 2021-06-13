﻿/*
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

using Fabrica.Utilities.Container;
using Fabrica.Watch;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ILogger = Fabrica.Watch.ILogger;

namespace Fabrica.Persistence.Contexts
{


    public abstract class BaseDbContext: DbContext
    {


        protected BaseDbContext( ICorrelation correlation, [NotNull] DbContextOptions options, ILoggerFactory factory=null ): base( options )
        {

            Correlation = correlation;

            Factory = factory;


        }


        protected ICorrelation Correlation { get; }

        private ILoggerFactory Factory { get; }

        protected override void OnConfiguring( [NotNull] DbContextOptionsBuilder optionsBuilder )
        {

            base.OnConfiguring(optionsBuilder);
            if( Factory != null )
                optionsBuilder.UseLoggerFactory(Factory);

        }

        public virtual void AfterCreate()
        {

        }


        protected ILogger GetLogger()
        {

            var logger = Correlation.GetLogger(this);

            return logger;

        }



    }


}