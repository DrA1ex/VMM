using System;
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
            WaveOut.PlaybackStopped += WaveOutOnPlaybackStopped;
        }

        private SynchronizationContext SynchronizationContext { get; } = SynchronizationContext.Current ?? new SynchronizationContext();
        private CancellationTokenSource CancellationTokenSource { get; set; }

        private WaveOutEvent WaveOut { get; }
        private Mp3FileReaderEx CurrentReader { get; set; }

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
            WaveOut.PlaybackStopped -= WaveOutOnPlaybackStopped;
            WaveOut.Dispose();
        }

        private void WaveOutOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
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

            CacheHelper.Download(entry, token).ContinueWith(task =>
            {
                try
                {
                    OnPlaybackStateChanged(PlaybackState.Playing);

                    CurrentReader = new Mp3FileReaderEx(task.Result, entry.Duration);

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
            WaveOut.PlaybackStopped -= WaveOutOnPlaybackStopped;
            WaveOut.Stop();
            WaveOut.PlaybackStopped += WaveOutOnPlaybackStopped;

            if(CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource.Dispose();
                CancellationTokenSource = null;
            }

            if(CurrentReader != null)
            {
                CurrentReader.Dispose();
                CurrentReader = null;
            }

            OnPlaybackStateChanged(PlaybackState.Stopped);
        }

        public event EventHandler<PlaybackState> PlaybackStateChanged;
        public event EventHandler PlaybackFinished;
        public event EventHandler<MusicPlayerEngineException> PlaybackFailed;

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
    }
}