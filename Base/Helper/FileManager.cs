using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Garm.Base.Interfaces;

namespace Garm.Base.Helper
{
    public class FileManager : Abstract.Base
    {
        protected Dictionary<string, MemoryStreamMultiplexer> Cache;
        protected Dictionary<string, int> Requests;
        protected Thread CleanUpThread;
        protected string DataFolder;

        public FileManager(IRunManager manager) : base(manager)
        {
            Cache = new Dictionary<string, MemoryStreamMultiplexer>();
            Requests = new Dictionary<string, int>();
            CleanUpThread = ThreadHelper.Start(CleanUpLoop, "FileManager_CleanUp");
            DataFolder = Path.Combine(
                    Manager.Opts.Get<bool>("sys_useDataSpecialFolder") ? Environment.GetFolderPath(Manager.Opts.Get<Environment.SpecialFolder>("sys_dataSpecialFolder")) : "",
                    Manager.Opts.Get<string>("sys_dataFolder"));
        }

        public MemoryStreamReader Get(string path)
        {
            if (Requests.ContainsKey(path.ToLower()))
            {
                lock (Requests)
                {
                    Requests[path.ToLower()]++;
                }
                lock (Cache)
                {
                    var memStream = Cache[path.ToLower()];
                    return memStream.GetReader();
                }
            }
            string realpath;
            if (path.Length > 2 && path.Substring(0, 2).ToLower().Equals("%%"))
                realpath = Path.Combine(DataFolder, path.Substring(2));
            else
                realpath = path;
            
            if (File.Exists(realpath))
            {
                var fileStream = File.Open(realpath, FileMode.Open);
                var memStream = new MemoryStreamMultiplexer();
                var bbuffer = new byte[fileStream.Length];
                fileStream.Read(bbuffer, 0, bbuffer.Length);
                memStream.Write(bbuffer,0,bbuffer.Length);
                fileStream.Close();
                fileStream.Dispose();
                lock (Requests)
                {
                    Requests[path.ToLower()] = 1;
                }
                lock (Cache)
                {
                    Cache[path.ToLower()] = memStream;
                }
                return memStream.GetReader();
            }
            //ToDo: Custom compressed FS here
#if DEBUG
            Console.WriteLine("Warning: Could not find requested File '" + path + "'");
#endif
            return null;
        }

        protected void CleanUpLoop()
        {
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
            Manager.WaitOnShutdown++;
            while (Manager.DoRun)
            {
                Manager.AbortOnExit.WaitOne(Manager.Opts.Get<int>("sys_files_cacheThreshold"));
                Dictionary<string, MemoryStreamMultiplexer> newCache;
                lock (Requests)
                {
                    var newRequests = Requests.Select(old => new KeyValuePair<string, int>(old.Key, old.Value - 1)).Where(
                    entry => entry.Value > 0).ToDictionary(x => x.Key, x => x.Value);
                    newCache = Requests.Keys.ToDictionary(key => key, key => Cache[key]);
                    Requests = newRequests;
                    foreach (KeyValuePair<string, MemoryStreamMultiplexer> file in Cache)
                    {
                        if(!newCache.ContainsKey(file.Key))
                            if (file.Value.ActiveReaderCount > 0)
                            {//File will not be unloaded yet anyway because there is at least one Reader still active, so we can keep it as well
                                newCache.Add(file.Key, file.Value);
                                Requests.Add(file.Key, 1);
                            }
                            else
                                file.Value.Dispose();
                    }
                }
                lock (Cache)
                {
                    Cache = newCache;
                }
            }
            Manager.WaitOnShutdown--;
        }

        public override void Dispose()
        {
            if(CleanUpThread.ThreadState != ThreadState.Stopped && 
                CleanUpThread.ThreadState != ThreadState.Unstarted)
                CleanUpThread.Abort();
            Cache.Clear();
            Requests.Clear();
        }

        public void ClearCache()
        {
            lock (Cache)
            {
                Cache.Clear();
            }
            lock (Requests)
            {
                Requests.Clear();
            }
        }
    }
}
