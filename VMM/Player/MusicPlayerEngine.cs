using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using VMM.Model;
using VMM.Player.Exceptions;
using VMM.Player.Helper;
using VMM.Player.Reader;

namespace VMM.Player
{
    public class MusicPlayerEngine : IDisposable
    {
        public MusicPlayerEngine()
        {
            WaveOut = new WaveOutEvent();
            WaveOut.PlaybackStopped += OnPlaybackFinished;
        }

        private SynchronizationContext SynchronizationContext { get; } = SynchronizationContext.Current ?? new SynchronizationContext();
        private CancellationTokenSource CancellationTokenSource { get; set; }

        private WaveOutEvent WaveOut { get; }
        private Mp3FileReaderEx CurrentReader { get; set; }
        private IBufferedObservable CurrentBufferedStream { get; set; }

        private bool _isManualStoped;

        private ConcurrentQueue<Mp3FileReaderEx> ReadersToDispose { get; } = new ConcurrentQueue<Mp3FileReaderEx>();

#pragma warning disable 612
        public float Volume
        {
            get { return WaveOut.Volume; }
            set { WaveOut.Volume = value; }
        }
#pragma warning restore 612

        public double Seek
        {
            get { return CurrentReader != null && WaveOut.PlaybackState != PlaybackState.Stopped ? (double)CurrentReader.Position / CurrentReader.Length : 0.0; }
            set { Task.Run(() => CurrentReader?.Seek((long)(value * CurrentReader.Length), SeekOrigin.Begin)); }
        }

        public void Dispose()
        {
            WaveOut.PlaybackStopped -= OnPlaybackFinished;
            WaveOut.Dispose();
        }

        private void OnPlaybackFinished(object sender, StoppedEventArgs stoppedEventArgs)
        {
            while(!ReadersToDispose.IsEmpty)
            {
                Mp3FileReaderEx reader;
                if(ReadersToDispose.TryDequeue(out reader))
                {
                    reader.Dispose();
                }
            }

            if(_isManualStoped)
            {
                _isManualStoped = false;
                return;
            }

            if(stoppedEventArgs.Exception == null)
            {
                OnSongFinished();
            }
            else
            {
                OnError(new MusicPlayerEngineException(stoppedEventArgs.Exception is InvalidOperationException
                    ? EngineError.WrongFileFormat
                    : EngineError.Unknown,
                    "Playback stoped with error", stoppedEventArgs.Exception));
            }
        }

        public void PlayPause()
        {
            if(WaveOut.PlaybackState == PlaybackState.Playing)
            {
                WaveOut.Pause();
                OnPlaybackStateChanged(PlaybackState.Paused);
            }
            else if(WaveOut.PlaybackState == PlaybackState.Paused)
            {
                WaveOut.Play();
                OnPlaybackStateChanged(PlaybackState.Playing);
            }
        }

        public void Play(MusicEntry entry)
        {
            Stop();

            CancellationTokenSource = new CancellationTokenSource();
            var token = CancellationTokenSource.Token;

            OnPlaybackStateChanged(PlaybackState.Playing);

            CacheHelper.Download(entry, token).ContinueWith(task =>
            {
                try
                {
                    var steam = task.Result;

                    CurrentBufferedStream = steam as IBufferedObservable;
                    if(CurrentBufferedStream != null)
                    {
                        CurrentBufferedStream.Buffed += OnBuffred;
                    }

                    OnBuffred(this, 0);
                    CurrentReader = new Mp3FileReaderEx(steam, entry.Duration);

                    token.ThrowIfCancellationRequested();

                    WaveOut.Init(CurrentReader);
                    WaveOut.Play();
                }
                catch(WebException e)
                {
                    OnError(new MusicPlayerEngineException(EngineError.NetworkError, e));
                }
                catch(OperationCanceledException)
                {
                    //ignore
                }
                catch(Exception e)
                {
                    OnError(new MusicPlayerEngineException(EngineError.Unknown, e));
                }
            }, token);
        }

        public void Stop()
        {
            if(CurrentReader != null)
            {
                ReadersToDispose.Enqueue(CurrentReader);
            }
            if(CurrentBufferedStream != null)
            {
                CurrentBufferedStream.Buffed -= OnBuffred;
                CurrentBufferedStream = null;
            }

            //event will be raised only if playback isn' stoped
            if(WaveOut.PlaybackState != PlaybackState.Stopped)
            {
                _isManualStoped = true;
            }
            WaveOut.Stop();

            if(CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource.Dispose();
                CancellationTokenSource = null;
            }

            OnPlaybackStateChanged(PlaybackState.Stopped);
        }

        public event EventHandler<PlaybackState> PlaybackStateChanged;
        public event EventHandler PlaybackFinished;
        public event EventHandler<MusicPlayerEngineException> PlaybackFailed;
        public event EventHandler<double> Buffred;

        protected virtual void OnSongFinished()
        {
            SynchronizationContext.Post(sender => PlaybackFinished?.Invoke(sender, EventArgs.Empty), this);
        }

        protected virtual void OnError(MusicPlayerEngineException e)
        {
            SynchronizationContext.Post(sender => PlaybackFailed?.Invoke(sender, e), this);
        }

        protected virtual void OnPlaybackStateChanged(PlaybackState state)
        {
            SynchronizationContext.Post(sender => PlaybackStateChanged?.Invoke(sender, state), this);
        }

        protected virtual void OnBuffred(object sender, long e)
        {
            var bufferedPercentage = (double)e / CurrentBufferedStream?.Length;
            SynchronizationContext.Post(s => Buffred?.Invoke(s, bufferedPercentage ?? 0), sender);
        }
    }
}