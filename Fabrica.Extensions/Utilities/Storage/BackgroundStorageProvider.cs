using System.Collections.Concurrent;
using Fabrica.Utilities.Container;
using Fabrica.Watch;

namespace Fabrica.Utilities.Storage
{


    public class BackgroundStorageProvider : IRemoteStorageProvider, IRequiresStart, IDisposable
    {


        protected class Request
        {

            public string Root { get; init; } = null!;
            public string Key { get; init; } = null!;

            public MemoryStream Content { get; } = new();

        }


        public BackgroundStorageProvider(IRemoteStorageProvider inner)
        {

            Inner = inner;

        }


        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(100);
        public TimeSpan StopInterval { get; set; } = TimeSpan.FromSeconds(20);


        private IStorageProvider Inner { get; }

        private ConcurrentQueue<Request> Queue { get; } = new ConcurrentQueue<Request>();


        public bool Exists(string root, string key)
        {
            return Inner.Exists(root, key);
        }

        public async Task<bool> ExistsAsync(string root, string key)
        {
            return await Inner.ExistsAsync(root, key);
        }

        public void Get(string root, string key, Stream content, bool rewind = true)
        {
            Inner.Get(root, key, content, rewind);
        }

        public async Task GetAsync(string root, string key, Stream content, bool rewind = true)
        {
            await Inner.GetAsync(root, key, content, rewind);
        }

        public void Put(string root, string key, Stream content, string contentType = "", bool rewind = true, bool autoClose = false)
        {

            ArgumentNullException.ThrowIfNull(content);
            ArgumentException.ThrowIfNullOrEmpty(root);
            ArgumentException.ThrowIfNullOrEmpty(key);

            using var logger = this.EnterMethod();



            logger.Inspect(nameof(root), root);
            logger.Inspect(nameof(key), key);



            // *****************************************************************
            logger.Debug("Attempting to create background request");
            var request = new Request
            {
                Root = root,
                Key = key
            };



            // *****************************************************************
            logger.Debug("Attempting to copy Content to request stream");
            content.CopyTo(request.Content);

            request.Content.Seek(0, SeekOrigin.Begin);



            // *****************************************************************
            logger.Debug("Attempting to enqueue Request");
            Queue.Enqueue(request);


        }

        public async Task PutAsync(string root, string key, Stream content, string contentType = "", bool rewind = true, bool autoClose = false)
        {

            ArgumentNullException.ThrowIfNull(content);
            ArgumentException.ThrowIfNullOrEmpty(root);
            ArgumentException.ThrowIfNullOrEmpty(key);

            using var logger = this.EnterMethod();



            // *****************************************************************
            logger.Debug("Attempting to create background request");
            var request = new Request
            {
                Root = root,
                Key = key
            };



            // *****************************************************************
            logger.Debug("Attempting to copy Content to request stream");
            await content.CopyToAsync(request.Content);

            request.Content.Seek(0, SeekOrigin.Begin);



            // *****************************************************************
            logger.Debug("Attempting to enqueue Request");
            Queue.Enqueue(request);



        }

        public void Delete(string root, string key)
        {
            Inner.Delete(root, key);
        }

        public async Task DeleteAsync(string root, string key)
        {
            await Inner.DeleteAsync(root, key);
        }

        public string GetReference(string root, string key, TimeSpan timeToLive)
        {
            return Inner.GetReference(root, key, timeToLive);
        }

        public async Task<string> GetReferenceAsync(string root, string key, TimeSpan timeToLive)
        {
            return await Inner.GetReferenceAsync(root, key, timeToLive);
        }

        public Task Start()
        {

            using var logger = this.EnterMethod();



            Task.Run(Poll);

            return Task.CompletedTask;

        }


        private ManualResetEvent MustStop { get; } = new (false);
        private ManualResetEvent HasStopped { get; } = new (false);
        private void Poll()
        {

            while (!MustStop.WaitOne(PollingInterval))
            {

                Request? req = null;
                try
                {

                    if (Queue.TryDequeue(out req))
                        Inner.Put(req.Root, req.Key, req.Content);

                }
                catch (Exception e)
                {
                    var errorLogger = this.GetLogger();
                    var ctx = new { Root = req?.Root ?? "", Key = req?.Key ?? "" };
                    errorLogger.ErrorWithContext(e, ctx, "Background Put failed");
                }

            }

            HasStopped.Set();

        }

        public void Dispose()
        {

            MustStop.Set();
            HasStopped.WaitOne(TimeSpan.FromSeconds(20));

            MustStop.Close();
            HasStopped.Close();

        }


    }


}
