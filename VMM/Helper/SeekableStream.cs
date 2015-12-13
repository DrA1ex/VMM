using System;
using System.IO;

namespace VMM.Helper
{
    public class SeekableStream : Stream
    {
        public SeekableStream(Stream stream, long length)
        {
            Stream = new ReadFullyStream(stream);
            InternalBuffer = new byte[length];
        }

        private Stream Stream { get; }
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
                var bytesToRead = Math.Min(InternalPosition - BufferedBytes + count, InternalBuffer.Length - BufferedBytes);
                BufferedBytes += Stream.Read(InternalBuffer, (int)BufferedBytes, (int)bytesToRead);
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
    }
}