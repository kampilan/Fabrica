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
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Fabrica.Utilities.Types
{


    public static class AssemblyExtensions
    {

        public static Stream GetResource( [NotNull] this Assembly target, [NotNull] string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return target.GetManifestResourceStream(name);
        }

        [NotNull]
        public static IEnumerable<string> GetResourceNames( this Assembly target, [NotNull] Func<string, bool> filter )
        {

            if (filter == null) throw new ArgumentNullException(nameof(filter));

            var results = target.GetManifestResourceNames().Where(filter);
            return results;
        
        }

        [NotNull]
        public static IEnumerable<string> GetResourceNamesByPath( this Assembly target, [NotNull] string path )
        {

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));


            bool Filter(string r) => r.StartsWith(path);

            var results = target.GetManifestResourceNames().Where(Filter);
            return results;

        }    

        [NotNull]
        public static IEnumerable<string> GetResourceNamesByExt( this Assembly target, [NotNull] string extension )
        {

            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(extension));

            bool Filter(string r) => r.EndsWith(extension);

            var results = target.GetManifestResourceNames().Where( Filter );
            return results;

        }


        [NotNull]
        public static IEnumerable<string> GetResourceNamesByPathAndExt( this Assembly target, [NotNull] string path, [NotNull] string extension )
        {

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));

            if (string.IsNullOrWhiteSpace(extension))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(extension));

            bool Filter(string r) => r.StartsWith(path) && r.EndsWith(extension);

            var results = target.GetManifestResourceNames().Where( Filter );
            return results;

        }


        [NotNull]
        public static IEnumerable<Type> GetFilteredTypes( this Assembly target, [NotNull] Func<Type, bool> filter )
        {

            if (filter == null) throw new ArgumentNullException(nameof(filter));

            return target.GetTypes().Where(filter);

        }

        [NotNull]
        public static IEnumerable<Type> GetImplementations( this Assembly target, [NotNull] Type implements )
        {

            if (implements == null) throw new ArgumentNullException(nameof(implements));

            bool Filter(Type t) => (t != implements) && (implements.IsAssignableFrom(t));

            return target.GetTypes().Where( Filter );

        }


        [NotNull]
        public static IEnumerable<Type> GetTypesWithAttribute( this Assembly target, [NotNull] Type attribute )
        {

            if (attribute == null) throw new ArgumentNullException(nameof(attribute));

            bool Filter(Type t) => t.GetCustomAttributes(attribute, false).Length > 0;

            return target.GetTypes().Where( Filter );

        }

        [NotNull]
        public static IEnumerable<Type> GetImplementationsWithAttribute( this Assembly target, [NotNull] Type implements, [NotNull] Type attribute )
        {

            if (implements == null) throw new ArgumentNullException(nameof(implements));
            if (attribute == null) throw new ArgumentNullException(nameof(attribute));

            bool Predicate(Type t) => (t != implements) && (implements.IsAssignableFrom(t)) && (t.GetCustomAttributes(attribute, false).Length > 0);

            return target.GetTypes().Where( Predicate );

        }


    }

}
