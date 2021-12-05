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
using System.Collections.Generic;

namespace Fabrica.Exceptions
{

    public abstract class ExternalException: Exception
    {


        protected ExternalException( string message ): base(message)
        {

            ErrorCode   = GetType().Name.Replace( "Exception", "" );
            Explanation = message;

        }


        protected ExternalException( string message, Exception inner ): base( message, inner )
        {

            ErrorCode   = GetType().Name.Replace("Exception", "");
            Explanation = message;

            if (!(inner is InternalException intra))
                return;

            InnerExplanation = intra.Explanation;

            foreach ( var detail in intra.Details )
                Details.Add( detail );

        }

        protected ExternalException( IExceptionInfo info ) : base(info.Explanation)
        {

            Kind             = info.Kind;
            ErrorCode        = GetType().Name.Replace("Exception", "");
            Explanation      = info.Explanation;
            InnerExplanation = info.Explanation;

            Details = new List<EventDetail>(info.Details);

        }



        public ErrorKind Kind        { get; protected set; } = ErrorKind.System;
        public string ErrorCode   { get; protected set; }
        public string Explanation { get; protected set; }
        public string InnerExplanation { get; protected set; }

        public string CorrelationId { get; protected set; } = "";


        public IList<EventDetail> Details { get; protected set; } = new List<EventDetail>();

    }

}
