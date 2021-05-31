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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Fabrica.Exceptions
{


    public class EventDetail
    {

        public enum EventCategory
        {
            Info,
            Warning,
            Violation,
            Error
        };

        public class Comparer : IEqualityComparer<EventDetail>
        {

            public bool Equals( EventDetail x, EventDetail y)
            {

                if (x == null || y == null)
                    return false;

                var eq = (x.Category == y.Category) && (x.RuleName == y.RuleName) && (x.Group == y.Group) && (x.Explanation == y.Explanation);
                return eq;

            }

            public int GetHashCode( EventDetail obj)
            {
                var hs = (obj.RuleName + obj.Group + obj.Explanation).GetHashCode();
                return hs;
            }

        }

        public static IEnumerable<EventDetail> DeDup( IEnumerable<EventDetail> source )
        {
            var set = new HashSet<EventDetail>( new Comparer() );

            set.UnionWith( source );

            return set;

        }

        public static IEnumerable<EventDetail> Merge( IEnumerable<EventDetail> source1, IEnumerable<EventDetail> source2 )
        {

            var set = new HashSet<EventDetail>(new Comparer());
            set.UnionWith( source1 );
            set.UnionWith( source2 );

            return set;

        }



        [NotNull]
        public static EventDetail Build()
        {
            return new EventDetail();
        }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EventCategory Category { get; set; } = EventCategory.Error;

        [DefaultValue("")]
        public string RuleName { get; set; } = "";

        [DefaultValue("")]
        public string Group { get; set; } = "";

        [DefaultValue("")]
        public string Source { get; set; } = "";

        [DefaultValue("")]
        public string Explanation { get; set; } = "";


        [NotNull]
        public EventDetail WithCategory( EventCategory category )
        {
            Category = category;
            return this;
        }


        [NotNull]
        public EventDetail WithRuleName([NotNull] string ruleName )
        {
            RuleName = ruleName ?? throw new ArgumentNullException(nameof(ruleName));
            return this;
        }


        [NotNull]
        public EventDetail WithGroup([NotNull] string group )
        {
            Group = group ?? throw new ArgumentNullException(nameof(group));
            return this;
        }


        [NotNull]
        public EventDetail WithSource([NotNull] object source )
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Source = source.ToString();
            return this;
        }


        [NotNull]
        public EventDetail WithExplanation([NotNull] string message )
        {
            Explanation = message ?? throw new ArgumentNullException(nameof(message));
            return this;
        }


    }


}
