using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Garm.Options
{
    internal interface IEntry
    {
        Type DataType { get; }
        object Data { get; set; }
        bool Readonly { get; }
        bool Cheat { get; }
        bool MpSync { get; }
    }

    internal interface IEntry<TData> : IEntry
    {
        new TData Data { get; set; }
    }

    internal class Entry<TData> : IEntry
    {
        private TData _data;
        private readonly bool _readonly;
        private readonly bool _cheat;
        private readonly bool _mpSync;

        internal Entry(TData content, bool cheat = false, bool mpSync = false, bool @readonly = false)//The @-sign marks this to be the variable-name instead of the keyword
        {
            _data = content;
            _readonly = @readonly;
            _cheat = cheat;
            _mpSync = mpSync;
        }

        public Type DataType
        {
            get { return typeof(TData); }
        }

        public object Data
        {
            get { return _data; }
            set
            {
                if (_readonly)
                    throw new ReadOnlyException();
                if (!(value is TData))
                    throw new ArgumentException();

                _data = (TData)value;
            }
        }

        public bool Readonly
        {
            get { return _readonly; }
        }

        public bool Cheat
        {
            get { return _cheat; }
        }

        public bool MpSync
        {
            get { return _mpSync; }
        }
    }
}
