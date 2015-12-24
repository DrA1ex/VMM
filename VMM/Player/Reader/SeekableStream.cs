using System;
using System.IO;
using System.Net;
using System.Threading;

namespace VMM.Player.Reader
{
    public class SeekableStream : Stream, IBufferedObservable
    {
        private const int BufferedRaiseThreshold = 1024 * 100; //Every 100 KiB
        private const int BufferPerRead = BufferedRaiseThreshold;

        private ManualResetEventSlim BufferedResetEvent { get; } = new ManualResetEventSlim(false);
        private Exception StreamBufferingException { get; set; }

        public SeekableStream(Stream stream, long length)
        {
            Stream = stream;
            InternalBuffer = new byte[length];

            BufferWholeStream();
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
        public event EventHandler<Exception> BufferingFailed;
        public override long Length => InternalBuffer.Length;

        public event EventHandler<long> Buffed;

        public byte[] GetBuffer => InternalBuffer;

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

        private async void BufferWholeStream()
        {
            try
            {
                var ct = ReadCancellationSource.Token;

                while(BufferedBytes < Length)
                {
                    var needToRead = Math.Min(BufferedBytes + BufferPerRead, Length - BufferedBytes);
                    var readed = await Stream.ReadAsync(InternalBuffer, (int)BufferedBytes, (int)needToRead, ct);
                    BufferedBytes += readed;

                    if(BufferedBytes - LastBufferedEventValue > BufferedRaiseThreshold)
                    {
                        OnBuffed(BufferedBytes);
                        LastBufferedEventValue = BufferedBytes;
                    }

                    BufferedResetEvent.Set();
                }
            }
            catch(WebException e) when(e.Status == WebExceptionStatus.RequestCanceled)
            {
                StreamBufferingException = new EndOfStreamException("Stream was closed because underlying WebRequest was canceled"
                    , new OperationCanceledException("WebRequest was canceled", e));
            }
            catch(Exception e)
            {
                StreamBufferingException = new EndOfStreamException("Unable to read from stream", e);
            }

            if(StreamBufferingException != null)
            {
                OnBufferingFailed(StreamBufferingException);
            }

            if(LastBufferedEventValue != BufferedBytes)
            {
                OnBuffed(BufferedBytes);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(StreamBufferingException != null)
            {
                if(StreamBufferingException is EndOfStreamException)
                {
                    throw StreamBufferingException;
                }
                throw new EndOfStreamException("Buffering failed with error", StreamBufferingException);
            }

            if(NeedWaitBuffer(count))
            {
                while(NeedWaitBuffer(count))
                {
                    BufferedResetEvent.Wait();
                }

                BufferedResetEvent.Reset();
            }

            var canRead = Math.Min(InternalBuffer.Length - InternalPosition, count);
            Array.Copy(InternalBuffer, InternalPosition, buffer, offset, canRead);

            InternalPosition += canRead;

            return (int)canRead;
        }

        private bool NeedWaitBuffer(int bytesRequested) => BufferedBytes < Math.Min(InternalPosition + bytesRequested, Length);

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

        protected virtual void OnBufferingFailed(Exception e)
        {
            BufferingFailed?.Invoke(this, e);
        }
    }
}