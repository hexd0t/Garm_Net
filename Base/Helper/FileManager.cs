using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Garm.Base.Interfaces;
using ThreadState = System.Threading.ThreadState;

namespace Garm.Base.Helper
{
    public partial class FileManager : Abstract.Base
    {
        protected Dictionary<string, MemoryStreamMultiplexer> Cache;
        protected Dictionary<string, int> Requests;
        protected Thread CleanUpThread;
        protected string DataFolder;
        protected int DirInfoMaxRecursionDepth;
        protected IEqualityComparer<string> FileNameComparer; 

        public FileManager(IRunManager manager) : base(manager)
        {
            Cache = new Dictionary<string, MemoryStreamMultiplexer>();
            Requests = new Dictionary<string, int>();
            CleanUpThread = ThreadHelper.Start(CleanUpLoop, "FileManager_CleanUp");
            FileNameComparer = new CaseinsensitiveEqualityComparer();
            DataFolder = Path.Combine(
                    Manager.Opts.Get<bool>("sys_useDataSpecialFolder") ? Environment.GetFolderPath(Manager.Opts.Get<Environment.SpecialFolder>("sys_dataSpecialFolder")) : "",
                    Manager.Opts.Get<string>("sys_dataFolder"));
            DirInfoMaxRecursionDepth = Manager.Opts.Get<int>("sys_dirInfo_maxRecursionDepth");
            Manager.Opts.RegisterChangeNotification("sys_dirInfo_maxRecursionDepth", delegate(string key, object value) { DirInfoMaxRecursionDepth = ( (int) value ); });
        }

        public bool Exists(string path)
        {
            if (Requests.ContainsKey(path))//File is cached, so it must exist
                return true;
            string userdatapath = Path.Combine(DataFolder, path);
            if (File.Exists(userdatapath))
                return true;
            if (File.Exists(path))
                return true;
            return false;//ToDo: check in compressed FS & via network
        }

        public bool DirExists(string path)
        {
            string userdatapath = Path.Combine(DataFolder, path);
            if (Directory.Exists(userdatapath))
                return true;
            if (Directory.Exists(path))
                return true;
            return false;//ToDo: check in compressed FS & via network
        }

        public MemoryStreamReader Get(string path, bool cache = true)
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

            string userdatapath = Path.Combine(DataFolder, path);

            if (File.Exists(userdatapath))
                return GetLocalFSFile(userdatapath, cache);
            if (File.Exists(path))
                return GetLocalFSFile(path, cache);
            //ToDo: Custom compressed FS here
            //ToDo: Network FS here
#if DEBUG
            Console.WriteLine("Warning: Could not find requested File '" + path + "'");
#endif
            return null;
        }

        public DirectoryInfo GetDir(string path, FileSource source = FileSource.All, int remainingdepth = -1)
        {
            if(remainingdepth == -1)
                remainingdepth = DirInfoMaxRecursionDepth;
            var result = new DirectoryInfo
            {
                Subdirectories = new List<DirectoryInfo>(),
                Files = new List<string>(),
                Name = String.IsNullOrWhiteSpace(path) ? "<>" : Path.GetFileName(path)
            };
            
            if(source.HasFlag(FileSource.LocalUserData) && !Path.IsPathRooted(path))
            {
                string userdatapath = Path.Combine(DataFolder, path);
                if (Directory.Exists(userdatapath) && remainingdepth > 0)
                {
                    foreach (string directory in Directory.GetDirectories(userdatapath))
                    {
                        var dirinfo = GetDir(directory, FileSource.Local, remainingdepth - 1); //Iterate as local since it's now a complete Path
                        result.Subdirectories.Add(dirinfo);
                    }
                }
                result.Files.AddRange(Directory.GetFiles(userdatapath).Select(Path.GetFileName));
            }

            if (source.HasFlag(FileSource.Local))
            {
                string localpath = Path.IsPathRooted(path) ? path : Path.Combine(Environment.CurrentDirectory, path);
                if (Directory.Exists(localpath) && remainingdepth > 0)
                {
                    foreach (string directory in Directory.GetDirectories(localpath))
                        result.Subdirectories.Add(GetDir(directory, FileSource.Local, remainingdepth - 1));
                }
                result.Files.AddRange(Directory.GetFiles(localpath).Select(Path.GetFileName));
            }

            //ToDo: Iterate on compressed FS and network FS

            return RemoveDoubleFiles(result);
        }

        protected DirectoryInfo RemoveDoubleFiles(DirectoryInfo di)
        {
            di.Files = di.Files.Distinct(FileNameComparer).ToList();
            for(int i = di.Subdirectories.Count - 1; i >= 0; i--)//Iterate backwards so only the indices of the already processed ones change
            {
                di.Subdirectories[i] = RemoveDoubleFiles(di.Subdirectories[i]);
                for(int j = i - 1; j >= 0; j--)//Won't calculate for i = 0 because there is nothing left to compare with
                {
                    if (di.Subdirectories[i].Name !=
                        di.Subdirectories[j].Name) continue;
                    di.Subdirectories[j].Subdirectories.AddRange(di.Subdirectories[i].Subdirectories);
                    di.Subdirectories[j].Files.AddRange(di.Subdirectories[i].Files);
                    di.Subdirectories.RemoveAt(i);
                    break;
                }
            }
            return di;
        }

        protected MemoryStreamReader GetLocalFSFile(string path, bool cache)
        {
            var fileStream = File.Open(path, FileMode.Open);
            var memStream = new MemoryStreamMultiplexer();
            var bbuffer = new byte[fileStream.Length];
            fileStream.Read(bbuffer, 0, bbuffer.Length);
            memStream.Write(bbuffer, 0, bbuffer.Length);
            fileStream.Close();
            fileStream.Dispose();

            lock (Requests)
            {
                Requests[path.ToLower()] = cache ? 1 : 0;
            }
            lock (Cache)
            {
                Cache[path.ToLower()] = memStream;
            }
            return memStream.GetReader();
        }

        protected void CleanUpLoop()
        {
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
            Manager.WaitOnShutdown++;
            while (Manager.AbortOnExit == null && Manager.DoRun)
                Thread.Sleep(100);
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
            base.Dispose();
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
