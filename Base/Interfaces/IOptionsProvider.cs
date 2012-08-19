using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Garm.Base.Interfaces
{
    public delegate void ValueChangedHandler(string key, object value);

    public interface IOptionsProvider : IDisposable
    {
        TData Get<TData>(string key, bool anticipatesNotFound = false);
        void Set<TData>(string key, TData value, bool runtime = true);
        ValueChangedHandler RegisterChangeNotification(string key, ValueChangedHandler handler);
        void UnregisterChangeNotification(ValueChangedHandler handler);
        Type GetType(string key);
        void ParseSet(string key, Type type, string value, bool runtime = true);
        /// <summary>
        /// Gets the count of defined variables
        /// Overridden variables count multiple times
        /// </summary>
        /// <returns>int[3] { Num Runtime Variables, Num User Variables, Num Default Variables }</returns>
        int[] GetStats();
    }
}
