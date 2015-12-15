/*
    This file based on NAudio Project
    https://github.com/naudio/NAudio
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NAudio.Wave;

//TODO: Need to refactor

namespace VMM.Helper
{
    internal class Mp3IndexedFrame
    {
        public Mp3IndexedFrame(Mp3Index index, Mp3Frame frame)
        {
            Index = index;
            Frame = frame;
        }

        public Mp3Index Index { get; set; }
        public Mp3Frame Frame { get; set; }
    }

    internal class TableOfContentBuilder : IEnumerable<Mp3IndexedFrame>
    {
        public TableOfContentBuilder(Stream mp3Stream, Mp3WaveFormat mp3WaveFormat)
        {
            Mp3Stream = mp3Stream;
            Mp3WaveFormat = mp3WaveFormat;
        }

        private List<Mp3IndexedFrame> Indexes { get; } = new List<Mp3IndexedFrame>();

        private Stream Mp3Stream { get; }

        public Mp3WaveFormat Mp3WaveFormat { get; }

        public IEnumerator<Mp3IndexedFrame> GetEnumerator()
        {
            var totalSamples = 0;

            foreach(var mp3Index in Indexes)
            {
                totalSamples += mp3Index.Index.SampleCount;
                yield return mp3Index;
            }

            Mp3Frame frame;
            do
            {
                var index = new Mp3Index();
                try
                {
                    index.FilePosition = Mp3Stream.Position;
                    index.SamplePosition = totalSamples;
                    frame = ReadNextFrame(true);
                    if(frame != null)
                    {
                        ValidateFrameFormat(frame);

                        totalSamples += frame.SampleCount;
                        index.SampleCount = frame.SampleCount;
                        index.ByteCount = (int)(Mp3Stream.Position - index.FilePosition);
                        Indexes.Add(new Mp3IndexedFrame(index, frame));
                    }
                }
                catch(EndOfStreamException)
                {
                    yield break;
                }

                yield return Indexes.Last();
            } while(frame != null);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Reads the next mp3 frame
        /// </summary>
        /// <returns>Next mp3 frame, or null if EOF</returns>
        private Mp3Frame ReadNextFrame(bool readData)
        {
            Mp3Frame frame = null;
            try
            {
                frame = Mp3Frame.LoadFromStream(Mp3Stream, readData);
            }
            catch(EndOfStreamException)
            {
                // suppress for now - it means we unexpectedly got to the end of the stream
                // half way through
            }
            return frame;
        }

        private void ValidateFrameFormat(Mp3Frame frame)
        {
            if(frame.SampleRate != Mp3WaveFormat.SampleRate)
            {
                var message =
                    $"Got a frame at sample rate {frame.SampleRate}, in an MP3 with sample rate {Mp3WaveFormat.SampleRate}. Mp3FileReader does not support sample rate changes.";
                throw new InvalidOperationException(message);
            }
            var channels = frame.ChannelMode == ChannelMode.Mono ? 1 : 2;
            if(channels != Mp3WaveFormat.Channels)
            {
                var message =
                    $"Got a frame with channel mode {frame.ChannelMode}, in an MP3 with {Mp3WaveFormat.Channels} channels. Mp3FileReader does not support changes to channel count.";
                throw new InvalidOperationException(message);
            }
        }
    }


    internal class Mp3Index
    {
        public long FilePosition { get; set; }
        public long SamplePosition { get; set; }
        public int SampleCount { get; set; }
        public int ByteCount { get; set; }
    }

    /// <summary>
    ///     Class for reading from MP3 files
    /// </summary>
    public class Mp3FileReaderEx : WaveStream
    {
        /// <summary>
        ///     Function that can create an MP3 Frame decompressor
        /// </summary>
        /// <param name="mp3Format">A WaveFormat object describing the MP3 file format</param>
        /// <returns>An MP3 Frame decompressor</returns>
        public delegate IMp3FrameDecompressor FrameDecompressorBuilder(WaveFormat mp3Format);

        private readonly int _bytesPerDecodedFrame;
        private readonly int _bytesPerSample;

        private readonly byte[] _decompressBuffer;

        private readonly bool _ownInputStream;

        private readonly object _repositionLock = new object();
        private int _decompressBufferOffset;
        private int _decompressLeftovers;

        private IMp3FrameDecompressor _decompressor;
        private Stream _mp3Stream;

        private long _position; // decompressed data position tracker
        private bool _repositionedFlag;
        private int _tocIndex;

        /// <summary>Supports opening a MP3 file</summary>
        public Mp3FileReaderEx(string mp3FileName)
            : this(File.OpenRead(mp3FileName), 0)
        {
            _ownInputStream = true;
        }

        /// <summary>Supports opening a MP3 file</summary>
        /// <param name="mp3FileName">MP3 File name</param>
        /// <param name="frameDecompressorBuilder">Factory method to build a frame decompressor</param>
        /// <param name="duration">Duration of the song in seconds</param>
        public Mp3FileReaderEx(string mp3FileName, FrameDecompressorBuilder frameDecompressorBuilder, int duration)
            : this(File.OpenRead(mp3FileName), frameDecompressorBuilder, duration)
        {
            _ownInputStream = true;
        }

        /// <summary>
        ///     Opens MP3 from a stream rather than a file
        ///     Will not dispose of this stream itself
        /// </summary>
        /// <param name="inputStream">The incoming stream containing MP3 data</param>
        /// <param name="duration">Duration of the song in seconds</param>
        public Mp3FileReaderEx(Stream inputStream, int duration)
            : this(inputStream, CreateAcmFrameDecompressor, duration)
        {
        }

        /// <summary>
        ///     Opens MP3 from a stream rather than a file
        ///     Will not dispose of this stream itself
        /// </summary>
        /// <param name="inputStream">The incoming stream containing MP3 data</param>
        /// <param name="frameDecompressorBuilder">Factory method to build a frame decompressor</param>
        /// <param name="duration">Duration of the song in seconds</param>
        public Mp3FileReaderEx(Stream inputStream, FrameDecompressorBuilder frameDecompressorBuilder, int duration)
        {
            if(inputStream == null) throw new ArgumentNullException(nameof(inputStream));
            try
            {
                _mp3Stream = inputStream;

                var dataStartPosition = _mp3Stream.Position;
                var firstFrame = Mp3Frame.LoadFromStream(_mp3Stream);
                if(firstFrame == null)
                    throw new InvalidDataException("Invalid MP3 file - no MP3 Frames Detected");
                double bitRate = firstFrame.BitRate;
                XingHeader = XingHeader.LoadXingHeader(firstFrame);
                // If the header exists, we can skip over it when decoding the rest of the file
                if(XingHeader != null) dataStartPosition = _mp3Stream.Position;

                // workaround for a longstanding issue with some files failing to load
                // because they report a spurious sample rate change
                var secondFrame = Mp3Frame.LoadFromStream(_mp3Stream);
                if(secondFrame != null &&
                   (secondFrame.SampleRate != firstFrame.SampleRate ||
                    secondFrame.ChannelMode != firstFrame.ChannelMode))
                {
                    // assume that the first frame was some kind of VBR/LAME header that we failed to recognise properly
                    dataStartPosition = secondFrame.FileOffset;
                    // forget about the first frame, the second one is the first one we really care about
                    firstFrame = secondFrame;
                }


                _mp3Stream.Position = dataStartPosition;

                // create a temporary MP3 format before we know the real bitrate
                Mp3WaveFormat = new Mp3WaveFormat(firstFrame.SampleRate,
                    firstFrame.ChannelMode == ChannelMode.Mono ? 1 : 2, firstFrame.FrameLength, (int)bitRate);

                CreateTableOfContents();

                var mp3DataLength = _mp3Stream.Length - dataStartPosition;


                if(duration > 0)
                {
                    TotalSamples = duration * Mp3WaveFormat.SampleRate;
                }
                else
                {
                    var lastSample = TableOfContents.Last();
                    TotalSamples = lastSample.Index.SamplePosition + lastSample.Index.SampleCount;
                }
                _tocIndex = 0;

                // [Bit rate in Kilobits/sec] = [Length in kbits] / [time in seconds] 
                //                            = [Length in bits ] / [time in milliseconds]

                // Note: in audio, 1 kilobit = 1000 bits.
                // Calculated as a double to minimize rounding errors
                bitRate = mp3DataLength * 8.0 / TotalSeconds();

                // now we know the real bitrate we can create an accurate MP3 WaveFormat
                Mp3WaveFormat = new Mp3WaveFormat(firstFrame.SampleRate,
                    firstFrame.ChannelMode == ChannelMode.Mono ? 1 : 2, firstFrame.FrameLength, (int)bitRate);
                _decompressor = frameDecompressorBuilder(Mp3WaveFormat);
                WaveFormat = _decompressor.OutputFormat;
                _bytesPerSample = _decompressor.OutputFormat.BitsPerSample / 8 * _decompressor.OutputFormat.Channels;
                // no MP3 frames have more than 1152 samples in them
                _bytesPerDecodedFrame = 1152 * _bytesPerSample;
                // some MP3s I seem to get double
                _decompressBuffer = new byte[_bytesPerDecodedFrame * 2];
            }
            catch(Exception)
            {
                if(_ownInputStream) inputStream.Dispose();
                throw;
            }
        }

        private IEnumerable<Mp3IndexedFrame> TableOfContents { get; set; }

        private long TotalSamples { get; }

        /// <summary>
        ///     The MP3 wave format (n.b. NOT the output format of this stream - see the WaveFormat property)
        /// </summary>
        public Mp3WaveFormat Mp3WaveFormat { get; }

        /// <summary>
        ///     This is the length in bytes of data available to be read out from the Read method
        ///     (i.e. the decompressed MP3 length)
        ///     n.b. this may return 0 for files whose length is unknown
        /// </summary>
        public override long Length => TotalSamples * _bytesPerSample;

        /// <summary>
        ///     <see cref="WaveStream.WaveFormat" />
        /// </summary>
        public override WaveFormat WaveFormat { get; }

        /// <summary>
        ///     <see cref="Stream.Position" />
        /// </summary>
        public override long Position
        {
            get { return _position; }
            set
            {
                lock(_repositionLock)
                {
                    value = Math.Max(Math.Min(value, Length), 0);
                    var samplePosition = value / _bytesPerSample;
                    Mp3Index mp3Index = null;

                    var index = 0;
                    foreach(var tableOfContentItem in TableOfContents)
                    {
                        if(tableOfContentItem.Index.SamplePosition + tableOfContentItem.Index.SampleCount > samplePosition)
                        {
                            mp3Index = tableOfContentItem.Index;
                            _tocIndex = index;
                            break;
                        }

                        ++index;
                    }

                    _decompressBufferOffset = 0;
                    _decompressLeftovers = 0;
                    _repositionedFlag = true;

                    if(mp3Index != null)
                    {
                        // set the offset into the buffer (that is yet to be populated in Read())
                        var frameOffset = samplePosition - mp3Index.SamplePosition;
                        if(frameOffset > 0)
                        {
                            _decompressBufferOffset = (int)frameOffset * _bytesPerSample;
                        }
                    }
                    else
                    {
                        _tocIndex = index; //Skip after last element
                    }

                    _position = value;
                }
            }
        }

        /// <summary>
        ///     Xing header if present
        /// </summary>
        public XingHeader XingHeader { get; }

        /// <summary>
        ///     Gets the total length of this file in milliseconds.
        /// </summary>
        private double TotalSeconds()
        {
            return (double)TotalSamples / Mp3WaveFormat.SampleRate;
        }

        /// <summary>
        ///     Creates an ACM MP3 Frame decompressor. This is the default with NAudio
        /// </summary>
        /// <param name="mp3Format">A WaveFormat object based </param>
        /// <returns></returns>
        public static IMp3FrameDecompressor CreateAcmFrameDecompressor(WaveFormat mp3Format)
        {
            // new DmoMp3FrameDecompressor(this.Mp3WaveFormat); 
            return new AcmMp3FrameDecompressor(mp3Format);
        }

        private void CreateTableOfContents()
        {
            TableOfContents = new TableOfContentBuilder(_mp3Stream, Mp3WaveFormat);
        }

        /// <summary>
        ///     Reads decompressed PCM data from our MP3 file.
        /// </summary>
        public override int Read(byte[] sampleBuffer, int offset, int numBytes)
        {
            if(_decompressor == null)
            {
                return 0; //Disposed
            }

            var bytesRead = 0;
            lock(_repositionLock)
            {
                if(_decompressLeftovers != 0)
                {
                    var toCopy = Math.Min(_decompressLeftovers, numBytes);
                    Array.Copy(_decompressBuffer, _decompressBufferOffset, sampleBuffer, offset, toCopy);
                    _decompressLeftovers -= toCopy;
                    if(_decompressLeftovers == 0)
                    {
                        _decompressBufferOffset = 0;
                    }
                    else
                    {
                        _decompressBufferOffset += toCopy;
                    }
                    bytesRead += toCopy;
                    offset += toCopy;
                }

                var targetTocIndex = _tocIndex; // the frame index that contains the requested data

                if(_repositionedFlag)
                {
                    _decompressor.Reset();

                    // Seek back a few frames of the stream to get the reset decoder decode a few
                    // warm-up frames before reading the requested data. Without the warm-up phase,
                    // the first half of the frame after the reset is attenuated and does not resemble
                    // the data as it would be when reading sequentially from the beginning, because 
                    // the decoder is missing the required overlap from the previous frame.
                    _tocIndex = Math.Max(0, _tocIndex - 3); // no warm-up at the beginning of the stream
                    //_mp3Stream.Position = TableOfContents.Skip(_tocIndex - 1).First().Index.FilePosition;

                    _repositionedFlag = false;
                }

                var contentEnumerable = TableOfContents.Skip(_tocIndex).GetEnumerator();

                while(bytesRead < numBytes)
                {
                    contentEnumerable.MoveNext();
                    if(contentEnumerable.Current == null)
                    {
                        return bytesRead; //End of stream
                    }

                    var frame = contentEnumerable.Current.Frame;
                    ++_tocIndex;
                    if(frame != null)
                    {
                        var decompressed = _decompressor.DecompressFrame(frame, _decompressBuffer, 0);

                        if(_tocIndex <= targetTocIndex || decompressed == 0)
                        {
                            // The first frame after a reset usually does not immediately yield decoded samples.
                            // Because the next instructions will fail if a buffer offset is set and the frame 
                            // decoding didn't return data, we skip the part.
                            // We skip the following instructions also after decoding a warm-up frame.
                            continue;
                        }
                        // Two special cases can happen here:
                        // 1. We are interested in the first frame of the stream, but need to read the second frame too
                        //    for the decoder to return decoded data
                        // 2. We are interested in the second frame of the stream, but because reading the first frame
                        //    as warm-up didn't yield any data (because the decoder needs two frames to return data), we
                        //    get data from the first and second frame. 
                        //    This case needs special handling, and we have to purge the data of the first frame.
                        if(_tocIndex == targetTocIndex + 1 && decompressed == _bytesPerDecodedFrame * 2)
                        {
                            // Purge the first frame's data
                            Array.Copy(_decompressBuffer, _bytesPerDecodedFrame, _decompressBuffer, 0, _bytesPerDecodedFrame);
                            decompressed = _bytesPerDecodedFrame;
                        }

                        var toCopy = Math.Min(decompressed - _decompressBufferOffset, numBytes - bytesRead);
                        Array.Copy(_decompressBuffer, _decompressBufferOffset, sampleBuffer, offset, toCopy);
                        if(toCopy + _decompressBufferOffset < decompressed)
                        {
                            _decompressBufferOffset = toCopy + _decompressBufferOffset;
                            _decompressLeftovers = decompressed - _decompressBufferOffset;
                        }
                        else
                        {
                            // no lefovers
                            _decompressBufferOffset = 0;
                            _decompressLeftovers = 0;
                        }
                        offset += toCopy;
                        bytesRead += toCopy;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            Debug.Assert(bytesRead <= numBytes, "MP3 File Reader read too much");
            _position += bytesRead;
            return bytesRead;
        }

        /// <summary>
        ///     Disposes this WaveStream
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(_mp3Stream != null)
                {
                    if(_ownInputStream)
                    {
                        _mp3Stream.Dispose();
                    }
                    _mp3Stream = null;
                }
                if(_decompressor != null)
                {
                    _decompressor.Dispose();
                    _decompressor = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}