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
using JetBrains.Annotations;

namespace Fabrica.Rql.Parser
{

    public class Target
    {


        public static bool operator ==( [CanBeNull] Target target, string candidate )
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));

            if ((String.IsNullOrWhiteSpace(target?.Name??"")) || String.IsNullOrWhiteSpace(candidate))
                return false;

            return String.Compare(target?.Name ?? "", candidate, StringComparison.InvariantCultureIgnoreCase) == 0;

        }

        public static bool operator !=( [CanBeNull] Target target, string candidate)
        {

            if ((String.IsNullOrWhiteSpace(target?.Name ?? "")) || String.IsNullOrWhiteSpace(candidate))
                return false;

            return String.Compare(target?.Name ?? "", candidate, StringComparison.InvariantCultureIgnoreCase) != 0;

        }


        public Target([NotNull] string name )
        {

            Name = name ?? throw new ArgumentNullException(nameof(name));

        }

        public string Name { get; }



        protected bool Equals([NotNull] Target other )
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            return string.Equals(Name, other.Name);

        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Target)obj);
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }


        public override string ToString()
        {
            return Name;
        }

    }

}
