﻿using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Fabrica.Watch;

namespace Fabrica.Utilities.Process
{
 
    public class FileSignalController: ISignalController, IStartable, IDisposable
    {

        public enum OwnerType { Host, Appliance }

        public FileSignalController( OwnerType owner, string path="" )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                logger.Inspect(nameof(owner), owner);
                logger.Inspect(nameof(path), path);


                Owner = owner;

                if( string.IsNullOrWhiteSpace(path) )
                {
                    logger.Debug("Attempting to calculate application entry directory");
                    var entry = Assembly.GetEntryAssembly()?.Location ?? "";
                    var fi    = new FileInfo(entry);
                    path = fi.DirectoryName??Path.GetTempPath();
                    logger.Inspect(nameof(path), path);
                }


                InstallationRoot = path;

                StartedFlag  = Path.Combine(path, "started.flag");
                StartedEvent = new ManualResetEvent(false);

                MustStopFlag = Path.Combine(path, "muststop.flag");
                MustStopEvent = new ManualResetEvent(false);

                StoppedFlag  = Path.Combine(path, "stopped.flag");
                StoppedEvent = new ManualResetEvent(false);

                EndWatchEvent = new ManualResetEvent(false);


            }
            finally
            {
                logger.LeaveMethod();
            }
            
        }

        private OwnerType Owner { get; }

        private string InstallationRoot { get; }


        private string StartedFlag { get; }
        private ManualResetEvent StartedEvent { get; }
        public bool WaitForStarted( TimeSpan interval ) => StartedEvent.WaitOne(interval);

        private string MustStopFlag { get; }
        private ManualResetEvent MustStopEvent { get; }
        public bool WaitForMustStop( TimeSpan interval ) => MustStopEvent.WaitOne(interval);

        private string StoppedFlag { get; }
        private ManualResetEvent StoppedEvent { get; }
        public bool WaitForStopped( TimeSpan interval ) => StoppedEvent.WaitOne(interval);

        private ManualResetEvent EndWatchEvent { get; }


        protected virtual void CreateSignal( SignalTypes type )
        {

            var logger = this.GetLogger();

            try
            {

                logger.EnterMethod();

                logger.Inspect(nameof(type), type);


                switch (type)
                {
                    case SignalTypes.Started:
                        Create(StartedFlag);
                        break;
                    case SignalTypes.MustStop:
                        Create( MustStopFlag );
                        break;
                    case SignalTypes.Stopped:
                        Create(StoppedFlag);
                        break;
                }

                void Create( string path )
                {

                    logger.Inspect(nameof(path), path);

                    // *****************************************************************
                    logger.Debug("Attempting to create signal file");
                    using (var writer = new StreamWriter(path))
                    {
                        writer.WriteLine("ok");
                    }
                }

            }
            finally
            {
                logger.LeaveMethod();
            }


        }


        public void Started()
        {
            CreateSignal(SignalTypes.Started);
            StartedEvent.Set();
        }

        public void RequestStop()
        {
            CreateSignal(SignalTypes.MustStop);
            MustStopEvent.Set();
        }

        public void Stopped()
        {
            CreateSignal(SignalTypes.Stopped);
            StoppedEvent.Set();
        }

        public void Reset()
        {

            foreach (var file in Directory.EnumerateFiles(InstallationRoot, "*.flag"))
                File.Delete(file);

            StartedEvent.Reset();
            MustStopEvent.Reset();
            StoppedEvent.Reset();

        }

        private void WatchHost()
        {

            while( true )
            {

                if( !StartedEvent.WaitOne(0) && CheckSignal(SignalTypes.Started) )
                    StartedEvent.Set();

                if (!StoppedEvent.WaitOne(0) && CheckSignal(SignalTypes.Stopped))
                {
                    StoppedEvent.Set();
                    break;
                }

                if (EndWatchEvent.WaitOne(TimeSpan.FromMilliseconds(500)))
                    break;

            }


        }

        private void WatchAppliance()
        {

            while (true)
            {

                if( !MustStopEvent.WaitOne(0) && CheckSignal(SignalTypes.MustStop) )
                {
                    MustStopEvent.Set();
                    break;
                }

                if (EndWatchEvent.WaitOne(TimeSpan.FromMilliseconds(500)))
                    break;

            }


        }

        protected virtual bool CheckSignal(SignalTypes type)
        {


            string path = null;
            switch (type)
            {
                case SignalTypes.Started:
                    path = StartedFlag;
                    break;
                case SignalTypes.MustStop:
                    path = MustStopFlag;
                    break;
                case SignalTypes.Stopped:
                    path = StoppedFlag;
                    break;
            }

            if (path == null)
                return false;

            var exists = File.Exists(path);

            return exists;

        }

        public bool HasStarted => CheckSignal(SignalTypes.Started);

        public bool MustStop => CheckSignal(SignalTypes.MustStop);

        public bool HasStopped => CheckSignal(SignalTypes.Stopped);


        public void Start()
        {
            if (Owner == OwnerType.Host)
                Task.Run(WatchHost);
            else
                Task.Run(WatchAppliance);
        }

        public void Dispose()
        {
            EndWatchEvent.Set();
        }

    }


}