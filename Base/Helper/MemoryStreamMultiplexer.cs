using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace Garm.Base.Helper
{
    /// <summary>
    /// Multithreaded buffer where one thread can write and many threads can read simultaneously. 
    /// </summary>
    public class MemoryStreamMultiplexer : IDisposable
    {
        public int ActiveReaderCount = 0;

        private bool _finished;
        private int _length;
        private List<byte[]> _buffer = new List<byte[]>();

        public int Length { get { return _length; } }

        public MemoryStreamMultiplexer()
        {
            
        }

        public void Write(byte[] data, int pos, int length)
        {
            byte[] newBuf = new byte[length];
            Buffer.BlockCopy(data, pos, newBuf, 0, length);
            lock (_buffer)
            {
                _buffer.Add(newBuf);
                _length += length;
            }
        }

        public MemoryStreamReader GetReader()
        {
            ActiveReaderCount++;        
            return new MemoryStreamReader(_buffer, this);
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                ActiveReaderCount = 0;
                
                disposed = true;
            }
        }
    }

    public class MemoryStreamReader : Stream, IDisposable
    {
        private int _position;
        private int _bufferIndex;
        private int _bufferPos;
        private List<byte[]> _bufferList;
        private MemoryStreamMultiplexer _multiplexer;
        
        public MemoryStreamReader(List<byte[]> bufferList, MemoryStreamMultiplexer multiplexer)
        {
            _bufferList = bufferList;
            _bufferPos = 0;
            _bufferIndex = 0;
            _position = 0;
            _multiplexer = multiplexer;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotSupportedException();    
        }

        public override long Length
        {
            get { return _bufferList.Sum(part => part.Length); }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {           
            if (_bufferIndex < _bufferList.Count)
            {
                return ReadInternal(buffer, offset, count);
            }
            return 0;   // No more bytes will be available. Finished.
        }

        private int ReadInternal(byte[] buffer, int offset, int count)
        {
            byte[] currentBuffer = _bufferList[_bufferIndex];

            if (_bufferPos + count <= currentBuffer.Length)
            {
                // the current buffer holds the same or more bytes than what is asked for
                // So, give what was asked.
                Buffer.BlockCopy(currentBuffer, _bufferPos, buffer, offset, count);

                _bufferPos += count;
                _position += count;
                return count;
            }
            else
            {
                // current buffer does not have the necessary bytes. deliver whatever is available.
                if (_bufferPos < currentBuffer.Length)
                {
                    int remainingBytes = currentBuffer.Length - _bufferPos;
                    Buffer.BlockCopy(currentBuffer, _bufferPos, buffer, offset, remainingBytes);

                    _position += remainingBytes;
                    _bufferIndex++;
                    _bufferPos = 0;

                    // Try to read from the next buffer in the list and deliver
                    // the undelivered bytes. The Read call might block and wait for 
                    // remaining bytes to appear. 
                    return remainingBytes + 
                        this.Read(buffer, offset + remainingBytes, count - remainingBytes);
                }
                else
                {
                    // Already all bytes from currnet buffer has been delivered. Try next buffer.
                    _bufferIndex++;
                    _bufferPos = 0;

                    // There may not be next buffer and thus we will have to wait.
                    return this.Read(buffer, offset, count);                    
                }
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public new void Dispose()
        {
            _multiplexer.ActiveReaderCount--;
            _bufferList = null;
        }
    }
}
