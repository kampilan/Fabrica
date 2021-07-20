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
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Fabrica.Utilities.Container;
using Fabrica.Utilities.Storage;
using Fabrica.Utilities.Threading;
using JetBrains.Annotations;

namespace Fabrica.Aws.Storage
{


    public class StorageComponent : CorrelatedObject, IRemoteStorageProvider
    {

        public StorageComponent( [NotNull] ICorrelation correlation, [NotNull] IAmazonS3 client) : base(correlation)
        {

            Client = client ?? throw new ArgumentNullException(nameof(client));

        }


        private IAmazonS3 Client { get; }


        public bool Exists(string root, string key)
        {

            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            using var logger = EnterMethod();

            return AsyncPump.Run(async () => await ExistsAsync(root, key));

        }

        public async Task<bool> ExistsAsync(string root, string key)
        {

            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            using var logger = EnterMethod();

            try
            {

                logger.Inspect(nameof(root), root);
                logger.Inspect(nameof(key), key);


                try
                {

                    // *****************************************************************
                    logger.Debug("Attempting to get Object Metadata");

                    var request = new GetObjectMetadataRequest
                    {
                        BucketName = root,
                        Key = key
                    };

                    var response = await Client.GetObjectMetadataAsync(request);

                    var found = response.HttpStatusCode == HttpStatusCode.OK;



                    // *****************************************************************
                    return found;

                }
                catch (AmazonS3Exception cause) when (cause.ErrorCode == "NotFound")
                {
                    return false;
                }


            }
            catch (Exception cause)
            {
                var context = new { Root = root, Key = key };
                logger.ErrorWithContext(cause, context, "Exists failed");
                throw;
            }

        }


        public void Get(string root, string key, Stream content, bool rewind = true)
        {

            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            if (content == null) throw new ArgumentNullException(nameof(content));

            using var logger = EnterMethod();


            AsyncPump.Run(async () => await GetAsync(root, key, content, rewind));


        }

        public async Task GetAsync(string root, string key, Stream content, bool rewind = true)
        {

            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));


            using var logger = EnterMethod();


            logger.Inspect("root", root);
            logger.Inspect("key", key);


            // *********************************************************************
            logger.Debug("Attempting to build S3 GetObjectRequest");
            var request = new GetObjectRequest
            {
                BucketName = root,
                Key = key
            };



            // *********************************************************************
            logger.Debug("Attempting to call GetObject from S3");
            var response = await Client.GetObjectAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                logger.LogObject("response", response);
                throw new IOException($"Could not Get item using Root=({root}) Key=({key})");
            }



            // *****************************************************************
            logger.Debug("Attempting to copy response stream to content");
            await using( var stream = response.ResponseStream )
            {
                await stream.CopyToAsync(content);
                if( rewind && content.CanSeek )
                    content.Seek(0, SeekOrigin.Begin);
            }


        }


        public void Put(string root, string key, Stream content, string contentType = "", bool rewind = true, bool autoClose = false)
        {

            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            if (content == null) throw new ArgumentNullException(nameof(content));

            using var logger = EnterMethod();


            AsyncPump.Run(async () => await PutAsync(root, key, content, contentType, rewind, autoClose));


        }

        public async Task PutAsync(string root, string key, Stream content, string contentType = "", bool rewind = true, bool autoClose = true)
        {

            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            if (content == null) throw new ArgumentNullException(nameof(content));


            using var logger = EnterMethod();


            logger.Inspect("root", root);
            logger.Inspect("key", key);



            // *********************************************************************
            logger.Debug("Attempting to build S3 PutObjectRequest");
            var request = new PutObjectRequest
            {
                AutoResetStreamPosition = rewind,
                AutoCloseStream = autoClose,
                BucketName = root,
                Key = key,
                ContentType = contentType,
                InputStream = content
            };



            // *********************************************************************
            logger.Debug("Attempting to call PutObject from S3");
            var response = await Client.PutObjectAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                logger.LogObject("response", response);
                throw new IOException($"Could not Put item using Root=({root}) Key=({key})");
            }


        }


        public void Delete(string root, string key)
        {

            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            using var logger = EnterMethod();

            AsyncPump.Run(async () => await DeleteAsync(root, key));


        }

        public async Task DeleteAsync(string root, string key)
        {

            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));


            using var logger = EnterMethod();


            logger.Inspect("root", root);
            logger.Inspect("key", key);


            // *********************************************************************
            logger.Debug("Attempting to build S3 GetObjectRequest");
            var request = new DeleteObjectRequest
            {
                BucketName = root,
                Key = key
            };



            // *********************************************************************
            logger.Debug("Attempting to call DeleteObject from S3");
            var response = await Client.DeleteObjectAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK)
                logger.LogObject("response", response);


        }


        public string GetReference(string root, string key, TimeSpan timeToLive)
        {

            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            using var logger = EnterMethod();

            return AsyncPump.Run(async () => await GetReferenceAsync(root, key, timeToLive));


        }

        public Task<string> GetReferenceAsync(string root, string key, TimeSpan timeToLive)
        {

            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));


            using var logger = EnterMethod();


            logger.Inspect(nameof(root), root);
            logger.Inspect(nameof(key), key);
            logger.Inspect(nameof(timeToLive), timeToLive.ToString());


            // *****************************************************************
            logger.Debug("Attempting to Get signed url");
            var request = new GetPreSignedUrlRequest
            {
                Protocol = Protocol.HTTPS,
                BucketName = root,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow + timeToLive,
                Key = key
            };


            var url = Client.GetPreSignedURL(request);

            logger.Inspect(nameof(url), url);



            // *****************************************************************
            return Task.FromResult(url);



        }


    }


}
