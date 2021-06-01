

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace Fabrica.Static.Providers.Mutable
{
    /// <summary>
    /// Looks up files using the on-disk file system
    /// </summary>
    /// <remarks>
    /// When the environment variable "DOTNET_USE_POLLING_FILE_WATCHER" is set to "1" or "true", calls to
    /// <see cref="Watch(string)" /> will use <see cref="PollingFileChangeToken" />.
    /// </remarks>
    public class MutableDirectoryFileProvider : IFileProvider, IDisposable
    {
   

        private static readonly char[] PathSeparators = new[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};

        private readonly ExclusionFilters _filters;


        public MutableDirectoryFileProvider(string root): this(root, ExclusionFilters.Sensitive)
        {
        }

        public MutableDirectoryFileProvider( string root, ExclusionFilters filters )
        {

            if( !Path.IsPathRooted(root) )
                throw new ArgumentException("The path must be absolute.", nameof(root));

            var fullRoot = Path.GetFullPath(root);
            // When we do matches in GetFullPath, we want to only match full directory names.
            Root = PathUtils.EnsureTrailingSlash(fullRoot);
            if (!Directory.Exists(Root))
                throw new DirectoryNotFoundException(Root);

            _filters = filters;

        }



        /// <summary>
        /// Disposes the provider. Change tokens may not trigger after the provider is disposed.
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Disposes the provider.
        /// </summary>
        /// <param name="disposing"><c>true</c> is invoked from <see cref="IDisposable.Dispose"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Destructor for <see cref="PhysicalFileProvider"/>.
        /// </summary>
        ~MutableDirectoryFileProvider() => Dispose(false);

        /// <summary>
        /// The root directory for this instance.
        /// </summary>
        public string Root { get; private set; }

        public void SetRoot( string root )
        {

            if (!Path.IsPathRooted(root))
                throw new ArgumentException("The path must be absolute.", nameof(root));

            var fullRoot = Path.GetFullPath(root);
            // When we do matches in GetFullPath, we want to only match full directory names.
            Root = PathUtils.EnsureTrailingSlash(fullRoot);
            if (!Directory.Exists(Root))
                throw new DirectoryNotFoundException(Root);

        }


        private string GetFullPath( string path )
        {

            if( PathUtils.PathNavigatesAboveRoot(path) )
                return null;

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath( Path.Combine(Root, path) );
            }
            catch
            {
                return null;
            }

            if( !IsUnderneathRoot(fullPath) )
                return null;

            return fullPath;

        }

        private bool IsUnderneathRoot(string fullPath)
        {
            return fullPath.StartsWith(Root, StringComparison.OrdinalIgnoreCase);
        }


        public IFileInfo GetFileInfo(string subpath)
        {

            if( string.IsNullOrEmpty(subpath) || PathUtils.HasInvalidPathChars(subpath) )
                return new NotFoundFileInfo(subpath);

            // Relative paths starting with leading slashes are okay
            subpath = subpath.TrimStart(PathSeparators);

            // Absolute paths not permitted.
            if( Path.IsPathRooted(subpath) )
                return new NotFoundFileInfo(subpath);

            var fullPath = GetFullPath(subpath);
            if (fullPath == null)
                return new NotFoundFileInfo(subpath);

            var fileInfo = new FileInfo(fullPath);
            if (FileSystemInfoHelper.IsExcluded(fileInfo, _filters))
                return new NotFoundFileInfo(subpath);


            return new PhysicalFileInfo(fileInfo);

        }

        /// <summary>
        /// Enumerate a directory at the given path, if any.
        /// </summary>
        /// <param name="subpath">A path under the root directory. Leading slashes are ignored.</param>
        /// <returns>
        /// Contents of the directory. Caller must check <see cref="IDirectoryContents.Exists"/> property. <see cref="NotFoundDirectoryContents" /> if
        /// <paramref name="subpath" /> is absolute, if the directory does not exist, or <paramref name="subpath" /> has invalid
        /// characters.
        /// </returns>
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            try
            {
                if (subpath == null || PathUtils.HasInvalidPathChars(subpath))
                {
                    return NotFoundDirectoryContents.Singleton;
                }

                // Relative paths starting with leading slashes are okay
                subpath = subpath.TrimStart(PathSeparators);

                // Absolute paths not permitted.
                if (Path.IsPathRooted(subpath))
                {
                    return NotFoundDirectoryContents.Singleton;
                }

                var fullPath = GetFullPath(subpath);
                if (fullPath == null || !Directory.Exists(fullPath))
                {
                    return NotFoundDirectoryContents.Singleton;
                }

                return new PhysicalDirectoryContents(fullPath, _filters);

            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (IOException)
            {
            }
            return NotFoundDirectoryContents.Singleton;
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }


    }


}

