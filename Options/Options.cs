using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Garm.Base.Interfaces;

namespace Garm.Options
{
    public sealed class Options : IOptionsProvider
    {
        private readonly Dictionary<string, IEntry> _valuesRuntime;
        private readonly Dictionary<string, IEntry> _valuesUser;
        private readonly Dictionary<string, IEntry> _valuesDefault;
        private readonly Dictionary<string, List<ValueChangedHandler>> _notifyHandlers;
        private ReaderWriterLockSlim _valuesRuntimeLock;
        private ReaderWriterLockSlim _valuesUserLock;
        private UserPreferences _preferenceFileHandler;
        private MethodInfo _setMethodInfo;
        private bool _startUpFinished;

        internal enum OptionLevel : byte { None = 0, Runtime, User, Default };

        public bool Cheats
        {
            get { return true; }
        }

        public Options()
        {
            _valuesRuntime = new Dictionary<string, IEntry>();
            _valuesUser = new Dictionary<string, IEntry>();
            _valuesDefault = new Dictionary<string, IEntry>();
            _notifyHandlers = new Dictionary<string, List<ValueChangedHandler>>();
            _valuesRuntimeLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _valuesUserLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _setMethodInfo = typeof(Options).GetMethod("Set");

            DefaultValues.LoadDefaults(ref _valuesDefault);
            _preferenceFileHandler = new UserPreferences(
                Path.Combine(
                    Get<bool>("sys_useDataSpecialFolder") ? Environment.GetFolderPath( Get<Environment.SpecialFolder>("sys_dataSpecialFolder") ) : "",
                    Get<string>("sys_userPreferences") ) );
            _preferenceFileHandler.Load(this);
        }

        public void LockDownReadonly()
        {
            _startUpFinished = true;//At this point, readonly-settings can no longer be changed
        }

        public TData Get<TData>(string key)
        {
            OptionLevel src;
            var data = Get(key, out src);
            if (data == null)
            {
                Console.WriteLine("[Error] Option '" + key + "':" + typeof(TData) + " not found!");
                return default(TData);
            }

            if (data.DataType != typeof(TData))
            {
                Console.WriteLine("[Error] Option '" + key + "'/" + Enum.GetName(typeof (OptionLevel), src) + " is a " +
                              data.DataType + ", not a " + typeof(TData));
                return default(TData);
            }

            return (TData)data.Data;
        }

        internal Dictionary<string, IEntry> GetListCopy(OptionLevel level)
        {
            switch (level)
            {
                case OptionLevel.Runtime:
                    _valuesRuntimeLock.EnterReadLock();
                    try
                    {
                        return new Dictionary<string, IEntry>(_valuesRuntime);
                    }
                    finally
                    {
                        _valuesRuntimeLock.ExitReadLock();
                    }
                case OptionLevel.User:
                    _valuesUserLock.EnterReadLock();
                    try
                    {
                        return new Dictionary<string, IEntry>(_valuesUser);
                    }
                    finally
                    {
                        _valuesUserLock.ExitReadLock();
                    }
                case OptionLevel.Default:
                    return new Dictionary<string, IEntry>(_valuesDefault);
            }
            return null;
        }

        public void Set<TData>(string key, TData value, bool runtime = true)
        {
            ReaderWriterLockSlim dictLock = runtime ? _valuesRuntimeLock : _valuesUserLock;
            OptionLevel src;

            var previous = Get(key, out src);
            bool cheat = false;
            bool mpSync = false;
            if (previous != null)
            {
                if (previous.Readonly && _startUpFinished)
                {
                    Console.WriteLine("[Warning] Tried to overwrite read-only option '" + key + "'/" +
                                        Enum.GetName(typeof (OptionLevel), src));
                    return;
                }
                if (previous.Cheat && !Cheats)
                {
                    Console.WriteLine("[Warning] Tried to overwrite cheat-protected option '" + key + "'/" +
                                        Enum.GetName(typeof (OptionLevel), src) + " while cheats disabled");
                    return;
                }
                if (src == OptionLevel.Runtime && !runtime)
                {
#if DEBUG
                    Console.WriteLine("[Info] Deleting '" + key + "' in Runtime, so new value in Users will become effective");
#endif
                    Remove(key, OptionLevel.Runtime);
                }
                cheat = previous.Cheat;
                mpSync = previous.MpSync;
            }

            dictLock.EnterWriteLock();
#if DEBUG
            if(_startUpFinished)
                Console.WriteLine("[Info] Setting '"+key+"'/"+(runtime?"R":"U")+" changed to '"+Base.Helper.StringHelpers.Truncate(value.ToString(),30)+"'");
#endif
            try
            {
                (runtime ? _valuesRuntime : _valuesUser)[key] = new Entry<TData>(value, cheat, mpSync);
            }
            finally { dictLock.ExitWriteLock(); }
            if (_notifyHandlers.ContainsKey(key))
            {
                foreach (var notifyHandler in _notifyHandlers[key])
                {
                    notifyHandler(key, value);
                }
            }
        }

        public void ParseSet(string key, Type type, string value, bool runtime = true)
        {
            var genericSet = _setMethodInfo.MakeGenericMethod(new[] { type });
            genericSet.Invoke(this, new[] { key, Parse(type, value), runtime });
        }

        public int[] GetStats()
        {
            _valuesRuntimeLock.EnterReadLock();
            var iRuntime = _valuesRuntime.Count;
            _valuesRuntimeLock.ExitReadLock();
            _valuesUserLock.EnterReadLock();
            var iUser = _valuesUser.Count;
            _valuesUserLock.ExitReadLock();
            return new[] { iRuntime, iUser, _valuesDefault.Count };
        }

        public static object Parse(Type type, string value)
        {
            object obj;

            MethodInfo parsemethod = type.GetMethod("Parse", new[] { typeof(string) });
            if (parsemethod != null)
            {
                obj = parsemethod.Invoke(null, new object[] { value });
            }
            else
            {
                obj = type.IsEnum ? Enum.Parse(type, value) : value;
            }
            return obj;
        }

        public ValueChangedHandler RegisterChangeNotification(string key, ValueChangedHandler handler)
        {
            if (!_notifyHandlers.ContainsKey(key))
                _notifyHandlers.Add(key, new List<ValueChangedHandler>());
            _notifyHandlers[key].Add(handler);
            return handler;
        }

        public void UnregisterChangeNotification(ValueChangedHandler handler)
        {
            foreach (var list in _notifyHandlers.Where(list => list.Value.Contains(handler)))
                list.Value.Remove(handler);
        }

        public Type GetType(string key)
        {
            OptionLevel src;
            var entry = Get(key, out src);
            if (entry == null)
                return null;
            return entry.DataType;
        }

        private IEntry Get(string key, out OptionLevel src)
        {
            _valuesRuntimeLock.EnterReadLock();
            try
            {
                if (_valuesRuntime.ContainsKey(key))
                {
                    src = OptionLevel.Runtime;
                    return _valuesRuntime[key];
                }
            }
            finally { _valuesRuntimeLock.ExitReadLock(); }

            _valuesUserLock.EnterReadLock();
            try
            {
                if (_valuesUser.ContainsKey(key))
                {
                    src = OptionLevel.User;
                    return _valuesUser[key];
                }
            }
            finally { _valuesUserLock.ExitReadLock(); }

            if (_valuesDefault.ContainsKey(key))
            {
                src = OptionLevel.Default;
                return _valuesDefault[key];
            }
            src = OptionLevel.None;
            return null;
        }

        private void Remove(string key, OptionLevel level)
        {
            switch (level)
            {
                case OptionLevel.Runtime:
                    _valuesRuntimeLock.EnterReadLock();
                    try
                    {
                        if (_valuesRuntime.ContainsKey(key))
                            _valuesRuntime.Remove(key);
                        else
                            Console.WriteLine("[Warning] Tried to remove nonexistant option '" + key + "'/" + Enum.GetName(typeof(OptionLevel), level));
                    }
                    finally { _valuesRuntimeLock.ExitReadLock(); }
                    break;
            }
        }

        public void Dispose()
        {
            _preferenceFileHandler.Save(this);
#if DEBUG
            foreach (var notifyHandlerList in _notifyHandlers)
            {
                if (notifyHandlerList.Value.Count > 0)
                {
                    Console.WriteLine("[Info] There are still " + notifyHandlerList.Value.Count + " notifyHandlers registered for '" + notifyHandlerList.Key + "'");
                }
            }
#endif
        }
    }
}
