using System;
using System.IO;

namespace VMM.Player.Reader
{
    public class ReadAheadStream<T> : Stream, IBufferedObservable
        where T : Stream, IBufferedObservable
    {
        public ReadAheadStream(T stream, int readAheadBytes)
        {
            Stream = stream;
            stream.Buffed += OnBuffed;
            BufferAhead = readAheadBytes;
        }

        public T Stream { get; }
        private long BufferAhead { get; }

        private readonly byte[] _dummyBuffer = new byte[1];

        public override bool CanRead => Stream.CanRead;
        public override bool CanSeek => Stream.CanSeek;
        public override bool CanWrite => Stream.CanWrite;
        public override long Length => Stream.Length;
        public long BufferedBytes => Stream.BufferedBytes;

        public override long Position
        {
            get { return Stream.Position; }
            set { Stream.Position = value; }
        }

        public event EventHandler<long> Buffed;

        public override void Flush()
        {
            Stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesToBuffer = Math.Min(Math.Max(0, BufferAhead - (Stream.BufferedBytes - Stream.Position)), Stream.Length - Stream.BufferedBytes);
            if(bytesToBuffer > 0)
            {
                var currentPosition = Stream.Position;
                Stream.Position = Stream.BufferedBytes + bytesToBuffer - 1;

                Stream.Read(_dummyBuffer, 0, 1);

                Stream.Position = currentPosition;
            }
            return Stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Stream.Write(buffer, offset, count);
        }

        protected virtual void OnBuffed(object sender, long e)
        {
            Buffed?.Invoke(sender, e);
        }
    }
}