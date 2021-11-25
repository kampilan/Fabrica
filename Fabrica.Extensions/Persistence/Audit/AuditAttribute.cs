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

namespace Fabrica.Persistence.Audit
{

    /// <summary>
    /// Controls data auditing when applied to a given type. The auditing is performed
    /// automatically during the course if ORM activites.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AuditAttribute: Attribute
    {

        public AuditAttribute()
        {
            Read     = false;
            Write    = true;
            Detailed = true;
        }


        /// <summary>
        /// Gets or sets a value indicating whether read operartion of the target type are
        /// recorded in the audit journal
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if read operatons will be audited ; otherwise, <see langword="false"/>.
        /// </value>
        public bool Read { get; set; }

        
        /// <summary>
        /// Gets or sets a value indicating whether writer operations are recorded in the
        /// audit journal
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if read operations will be audited ; otherwise, <see langword="false"/>.
        /// </value>
        public bool Write { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed (property level from -&gt; to)
        /// audit journaling should be performed.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if detailed audit should be performed ; otherwise, <see langword="false"/>.
        /// </value>
        public bool Detailed { get; set; }


    }

}
