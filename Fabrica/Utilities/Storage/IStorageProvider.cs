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
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Fabrica.Utilities.Storage
{

    
    public interface IStorageProvider
    {
       

        bool Exists( [NotNull] string root, [NotNull] string key );
        Task<bool> ExistsAsync( [NotNull] string root, [NotNull] string key );

        void Get( [NotNull] string root, [NotNull] string key, Stream content, bool rewind = true );
        Task GetAsync([NotNull] string root, [NotNull] string key, Stream content, bool rewind = true);

        void Put( [NotNull] string root, [NotNull] string key, [NotNull] Stream content, string contentType = "", bool rewind = true, bool autoClose = false );
        Task PutAsync([NotNull] string root, [NotNull] string key, [NotNull] Stream content, string contentType = "", bool rewind = true, bool autoClose = false);

        void Delete( [NotNull] string root, [NotNull] string key );
        Task DeleteAsync([NotNull] string root, [NotNull] string key);


        string GetReference( [NotNull] string root, [NotNull] string key, TimeSpan timeToLive );
        Task<string> GetReferenceAsync([NotNull] string root, [NotNull] string key, TimeSpan timeToLive);


    }


}
