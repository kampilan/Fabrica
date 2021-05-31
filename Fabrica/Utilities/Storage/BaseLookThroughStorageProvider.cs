using System;
using System.IO;
using System.Threading.Tasks;
using Fabrica.Watch;
using JetBrains.Annotations;

namespace Fabrica.Utilities.Storage
{


    public abstract class BaseLookThroughStorageProvider: IStorageProvider
    {


        protected  ILocalStorageProvider Local { get; set; }
        protected  IRemoteStorageProvider Remote { get; set; }

        public bool Exists(string root, string key)
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to check Local storage provider");
                var local = Local.Exists(root, key);
                if( local )
                {
                    logger.Debug( "Exists in Local" );
                    return true;
                }


                // *****************************************************************
                logger.Debug("Attempting to check in Remote storage provider");
                var remote = Remote.Exists(root, key);
                if( remote )
                {
                    logger.Debug("Exists in Remote");
                    return true;
                }



                // *****************************************************************
                return false;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        public async Task<bool> ExistsAsync( string root, string key )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();



                // *****************************************************************
                logger.Debug("Attempting to check Local storage provider");
                var local = await Local.ExistsAsync( root, key );
                if( local )
                {
                    logger.Debug("Exists in Local");
                    return true;
                }



                // *****************************************************************
                logger.Debug("Attempting to check in Remote storage provider");
                var remote = await Remote.ExistsAsync( root, key );
                if( remote )
                {
                    logger.Debug("Exists in Remote");
                    return true;
                }



                // *****************************************************************
                return false;


            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        public void Get( string root, string key, [NotNull] Stream content, bool rewind = true )
        {

            if (content == null) throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect(nameof(root), root);
                logger.Inspect(nameof(key), key);



                // *****************************************************************
                logger.Debug("Attempting to check local storage provider");
                if( Local.Exists( root, key ) )
                {
                    logger.Debug( "Item found by Local storage provider" );
                    Local.Get( root, key, content, rewind );
                    return;
                }



                // *****************************************************************
                logger.Debug("Attempting to Get from remote storage provider");
                Remote.Get( root, key, content, rewind );



                // *****************************************************************
                logger.Debug("Attempting to put to Local storage provider");
                Local.Put( root, key, content );

                if( content.CanSeek )
                    content.Seek(0, SeekOrigin.Begin);


            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        public async Task GetAsync(string root, string key, [NotNull] Stream content, bool rewind = true)
        {

            if (content == null) throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                logger.Inspect(nameof(root), root);
                logger.Inspect(nameof(key), key);



                // *****************************************************************
                logger.Debug("Attempting to check local storage provider");
                var exists = await Local.ExistsAsync( root, key );
                if( exists )
                {
                    logger.Debug("Item found by Local storage provider");
                    await Local.GetAsync( root, key, content, rewind );
                    return;
                }



                // *****************************************************************
                logger.Debug("Attempting to Get from Remote storage provider");
                await Remote.GetAsync(root, key, content, rewind);



                // *****************************************************************
                logger.Debug("Attempting to put to Local storage provider");
                Local.Put(root, key, content);

                if( content.CanSeek )
                    content.Seek(0, SeekOrigin.Begin);


            }
            finally
            {
                logger.LeaveMethod();
            }


        }

        public void Put(string root, string key, Stream content, string contentType = "", bool rewind = true, bool autoClose = false)
        {

            if (content == null) throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                logger.Inspect(nameof(root), root);
                logger.Inspect(nameof(key), key);



                // *****************************************************************
                logger.Debug("Attempting to Put to Local storage provider");
                Local.Put( root, key, content, contentType, rewind );



                // *****************************************************************
                logger.Debug("Attempting to Put to remote storage provider");
                Remote.Put( root, key, content, contentType, true, autoClose );


            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        public async Task PutAsync(string root, string key, Stream content, string contentType = "", bool rewind = true, bool autoClose = false)
        {

            if (content == null) throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                logger.Inspect(nameof(root), root);
                logger.Inspect(nameof(key), key);



                // *****************************************************************
                logger.Debug("Attempting to Put to Local storage provider");
                await Local.PutAsync(root, key, content, contentType, rewind);



                // *****************************************************************
                logger.Debug("Attempting to Put to remote storage provider");
                await Remote.PutAsync(root, key, content, contentType, true, autoClose);


            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        public void Delete(string root, string key)
        {

            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                // *****************************************************************
                logger.Debug("Attempting to Delete from Local storage provider");
                Local.Delete(root,key);


                // *****************************************************************
                logger.Debug("Attempting to Delete from Remote storage provider");
                Remote.Delete( root, key );


            }
            finally
            {
                logger.LeaveMethod();
            }

        }

        public async Task DeleteAsync(string root, string key)
        {

            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(root));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();


                // *****************************************************************
                logger.Debug("Attempting to Delete from Local storage provider");
                await Local.DeleteAsync(root, key);


                // *****************************************************************
                logger.Debug("Attempting to Delete from Remote storage provider");
                await Remote.DeleteAsync(root, key);


            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public string GetReference(string root, string key, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetReferenceAsync(string root, string key, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }


    }


}
