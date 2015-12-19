using System;
using System.IO;
using System.Net;
using System.Threading;

namespace VMM.Player.Reader
{
    public class SeekableStream : Stream, IBufferedObservable
    {
        private const int BufferedRaiseThreshold = 1024 * 100; //Every 100 KiB

        public SeekableStream(Stream stream, long length)
        {
            Stream = stream;
            InternalBuffer = new byte[length];
        }

        private CancellationTokenSource ReadCancellationSource { get; } = new CancellationTokenSource();

        private Stream Stream { get; }
        private byte[] InternalBuffer { get; }
        private long LastBufferedEventValue { get; set; }
        private long InternalPosition { get; set; }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;

        public override long Position
        {
            get { return InternalPosition; }
            set
            {
                if(value < InternalBuffer.Length)
                {
                    InternalPosition = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        public long BufferedBytes { get; private set; }
        public override long Length => InternalBuffer.Length;

        public event EventHandler<long> Buffed;

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch(origin)
            {
                case SeekOrigin.Begin:
                    InternalPosition = offset;
                    break;
                case SeekOrigin.Current:
                    InternalPosition += offset;
                    break;
                case SeekOrigin.End:
                    InternalPosition = InternalBuffer.Length - offset;
                    break;
            }
            if(InternalPosition < 0 || InternalPosition > InternalBuffer.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            return InternalPosition;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(BufferedBytes < InternalPosition + count)
            {
                var needToRead = Math.Min(InternalPosition - BufferedBytes + count, InternalBuffer.Length - BufferedBytes);
                try
                {
                    while(needToRead > 0)
                    {
                        var readed = Stream.ReadAsync(InternalBuffer, (int)BufferedBytes, (int)needToRead, ReadCancellationSource.Token).Result;
                        BufferedBytes += readed;
                        needToRead -= readed;
                    }
                }
                catch(WebException e) when(e.Status == WebExceptionStatus.RequestCanceled)
                {
                    throw new EndOfStreamException("Stream was closed because underlying WebRequest was canceled"
                        , new OperationCanceledException("WebRequest was canceled", e));
                }
                catch(Exception e)
                {
                    throw new EndOfStreamException("Unable to read from stream", e);
                }
                if(BufferedBytes - LastBufferedEventValue > BufferedRaiseThreshold)
                {
                    OnBuffed(BufferedBytes);
                    LastBufferedEventValue = BufferedBytes;
                }
            }

            var canRead = Math.Min(InternalBuffer.Length - InternalPosition, count);
            Array.Copy(InternalBuffer, InternalPosition, buffer, offset, canRead);

            InternalPosition += canRead;

            return (int)canRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            ReadCancellationSource.Cancel(true);
            ReadCancellationSource.Dispose();

            base.Dispose(disposing);
        }

        protected virtual void OnBuffed(long e)
        {
            Buffed?.Invoke(this, e);
        }
    }
}