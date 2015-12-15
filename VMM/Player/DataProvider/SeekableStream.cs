using System;
using System.IO;
using System.Threading;

namespace VMM.Player.DataProvider
{
    public class SeekableStream : System.IO.Stream
    {
        public SeekableStream(System.IO.Stream stream, long length)
        {
            Stream = stream;
            InternalBuffer = new byte[length];
        }

        private CancellationTokenSource ReadCancellationSource { get; } = new CancellationTokenSource();

        private System.IO.Stream Stream { get; }
        private byte[] InternalBuffer { get; }
        private long BufferedBytes { get; set; }
        private long InternalPosition { get; set; }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => InternalBuffer.Length;

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
                catch(Exception)
                {
                    return 0;
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
    }
}