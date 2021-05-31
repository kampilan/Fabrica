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

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Fabrica.Exceptions
{

    public abstract class FluentException<TDescendant>: ExternalException where TDescendant: FluentException<TDescendant>
    {

        protected FluentException(string message) : base(message)
        {
        }

        protected FluentException(string message, Exception inner) : base(message, inner)
        {
        }


        [NotNull]
        public TDescendant WithKind(ErrorKind kind)
        {
            Kind = kind;
            return (TDescendant)this;
        }

        [NotNull]
        public TDescendant WithErrorCode([NotNull] string code)
        {
            ErrorCode = code ?? throw new ArgumentNullException(nameof(code));
            return (TDescendant)this;
        }

        [NotNull]
        public TDescendant WithExplaination([NotNull] string explaination)
        {
            Explanation = explaination ?? throw new ArgumentNullException(nameof(explaination));
            return (TDescendant)this;

        }
        [NotNull]
        public TDescendant WithCorrelationId([NotNull] string correlationId)
        {
            CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
            return (TDescendant)this;

        }

        [NotNull]
        public TDescendant WithDetail([NotNull] EventDetail detail)
        {

            if (detail == null) throw new ArgumentNullException(nameof(detail));

            Details.Add(detail);
            return (TDescendant)this;

        }

        [NotNull]
        public TDescendant WithDetails([NotNull] IEnumerable<EventDetail> details)
        {
            if (details == null) throw new ArgumentNullException(nameof(details));

            foreach (var d in details)
                Details.Add(d);

            return (TDescendant)this;

        }


        [NotNull]
        public TDescendant With([NotNull] IExceptionInfo info )
        {

            if (info == null) throw new ArgumentNullException(nameof(info));

            this
                .WithKind(info.Kind)
                .WithErrorCode(info.ErrorCode)
                .WithExplaination(info.Explanation)
                .WithDetails(info.Details);


            return (TDescendant)this;

        }



    }


}
