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
using System.Linq;
using Fabrica.Exceptions;

namespace Fabrica.Rules
{

    public class EvaluationResults
    {

        public EvaluationResults()
        {
            Events = new HashSet<EventDetail>( new EventDetail.Comparer() );
            Shared = new Dictionary<string, object>();

            TotalEvaluated = 0;
            TotalFired = 0;

            Started = DateTime.Now;
            Completed = DateTime.MaxValue;

            FiredRules = new Dictionary<string, int>();

            MutexWinners = new Dictionary<string, string>();
        }

        public IDictionary<string, object> Shared { get; }


        public int TotalAffirmations { get; set; }

        public int TotalVetos { get; set; }

        public int Score => TotalAffirmations - TotalVetos;


        public ISet<EventDetail> Events { get;}

        public bool HasViolations
        {
            get { return Events.Count( e => e.Category == EventDetail.EventCategory.Violation ) > 0; }
        }

        public DateTime Started { get; set; }
        public DateTime Completed { get; set; }

        public long Duration => Convert.ToInt64( (Completed - Started).TotalMilliseconds );

        public int TotalEvaluated { get; set; }
        public int TotalFired { get; set; }

        public IDictionary<string, string> MutexWinners { get;  }
        public IDictionary<string, int> FiredRules { get;  }

        public void Affirm( int amount )
        {
            TotalAffirmations += amount;
        }

        public void Veto( int amount )
        {
            TotalVetos += amount;
        }

    }

}
