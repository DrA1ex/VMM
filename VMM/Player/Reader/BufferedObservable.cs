using System;
using System.IO;

namespace VMM.Player.Reader
{
    public class BufferedObservableStream<T> : Stream, IBufferedObservable
        where T : Stream, IBufferedObservable
    {
        public BufferedObservableStream(T stream, int bufferSize)
        {
            Stream = stream;
            stream.Buffed += OnBuffed;

            BufferedStream = new BufferedStream(Stream, bufferSize);
        }

        public T Stream { get; }

        private BufferedStream BufferedStream { get; }

        public override bool CanRead => BufferedStream.CanRead;
        public override bool CanSeek => BufferedStream.CanSeek;
        public override bool CanWrite => BufferedStream.CanWrite;
        public override long Length => BufferedStream.Length;

        public override long Position
        {
            get { return BufferedStream.Position; }
            set { BufferedStream.Position = value; }
        }

        public event EventHandler<long> Buffed;

        public override void Flush()
        {
            BufferedStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BufferedStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BufferedStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readed = BufferedStream.Read(buffer, offset, count);
            return readed;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BufferedStream.Write(buffer, offset, count);
        }

        protected virtual void OnBuffed(object sender, long e)
        {
            Buffed?.Invoke(sender, e);
        }
    }
}