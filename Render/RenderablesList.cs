using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Garm.Base.Helper;

namespace Garm.View.Human.Render
{
    public class RenderablesList : IList<IRenderable>, IDisposable
    {
        protected List<IRenderable> currentList { get; set; }
        protected List<IRenderable> nextList { get; set; }
        protected ReaderWriterLockSlim nextListLock;
        
        public RenderablesList()
        {
            currentList = new List<IRenderable>();
            nextList = new List<IRenderable>();
            nextListLock = new ReaderWriterLockSlim();
        }

        public ReadOnlyCollection<IRenderable> Current { get { return currentList.AsReadOnly(); } }

        public void Update()
        {
            nextListLock.EnterWriteLock();
            try
            {
                currentList = nextList;
                nextList = new List<IRenderable>(currentList.Count + 1);
                nextList.AddRange(currentList);
            }
            finally
            {
                nextListLock.ExitWriteLock();
            }
        }

        public IEnumerator<IRenderable> GetEnumerator()
        {
            nextListLock.EnterReadLock();
            try
            {
                return nextList.GetEnumerator();
            }
            finally
            {
                nextListLock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IRenderable item)
        {
            nextListLock.EnterWriteLock();
            try
            {
                nextList.Add(item);
            }
            finally
            {
                nextListLock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            nextListLock.EnterWriteLock();
            try
            {
                nextList.Clear();
            }
            finally
            {
                nextListLock.ExitWriteLock();
            }
        }

        public bool Contains(IRenderable item)
        {
            nextListLock.EnterReadLock();
            try
            {
                return nextList.Contains(item);
            }
            finally
            {
                nextListLock.ExitReadLock();
            }
        }

        public void CopyTo(IRenderable[] array, int arrayIndex)
        {
            nextListLock.EnterReadLock();
            try
            {
                nextList.CopyTo(array,arrayIndex);
            }
            finally
            {
                nextListLock.ExitReadLock();
            }
        }

        public bool Remove(IRenderable item)
        {
            nextListLock.EnterWriteLock();
            try
            {
                return nextList.Remove(item);
            }
            finally
            {
                nextListLock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                nextListLock.EnterReadLock();
                try
                {
                    return nextList.Count;
                }
                finally
                {
                    nextListLock.ExitReadLock();
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(IRenderable item)
        {
            nextListLock.EnterReadLock();
            try
            {
                return nextList.IndexOf(item);
            }
            finally
            {
                nextListLock.ExitReadLock();
            }
        }

        public void Insert(int index, IRenderable item)
        {
            nextListLock.EnterWriteLock();
            try
            {
                nextList.Insert(index, item);
            }
            finally
            {
                nextListLock.ExitWriteLock();
            }
        }

        public void RemoveAt(int index)
        {
            nextListLock.EnterReadLock();
            try
            {
                nextList.RemoveAt(index);
            }
            finally
            {
                nextListLock.ExitReadLock();
            }
        }

        public IRenderable this[int index]
        {
            get
            {
                nextListLock.EnterReadLock();
                try
                {
                    return nextList[index];
                }
                finally
                {
                    nextListLock.ExitReadLock();
                }
            }
            set
            {
                nextListLock.EnterWriteLock();
                try
                {
                    nextList[index] = value;
                }
                finally
                {
                    nextListLock.ExitWriteLock();
                }
            }
        }

        public void Dispose()
        {
            nextListLock.Dispose();
        }
    }
}
