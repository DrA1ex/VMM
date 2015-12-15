using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using JetBrains.Annotations;
using NAudio.Wave;
using VMM.Helper;
using VMM.Model;

namespace VMM.Utils
{
    public class MusicPlayer : IDisposable, INotifyPropertyChanged
    {
        private MusicEntry _currentSong;

        private DispatcherTimer _seekTimer;

        static MusicPlayer()
        {
            Instance = new MusicPlayer();
        }

        private MusicPlayer()
        {
            WaveOut = new WaveOutEvent();
            WaveOut.PlaybackStopped += OnPlaybackStopped;

            SeekTimer.Interval = TimeSpan.FromSeconds(0.33);
            SeekTimer.Tick += (sender, args) => OnPropertyChanged(nameof(Seek));
        }


        public static MusicPlayer Instance { get; private set; }

        private WaveOutEvent WaveOut { get; }
        private Mp3FileReaderEx Reader { get; set; }

        public DispatcherTimer SeekTimer => _seekTimer ?? (_seekTimer = new DispatcherTimer());

        public double Volume
        {
            get { return WaveOut.Volume; }
            set { WaveOut.Volume = (float)value; }
        }

        public double Seek
        {
            get { return Reader != null && WaveOut.PlaybackState != PlaybackState.Stopped ? (double)Reader.Position / Reader.Length : 0.0; }
            set { Task.Run(() => Reader?.Seek((long)(value * Reader.Length), SeekOrigin.Begin)); }
        }

        public MusicEntry CurrentSong
        {
            get { return _currentSong; }
            set
            {
                _currentSong = value;
                OnPropertyChanged(nameof(CurrentSong));
            }
        }

        private bool IsStopManualy { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            Reader?.Dispose();
            WaveOut.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        ~MusicPlayer()
        {
            Dispose();
        }

        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        protected virtual void OnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            //PlaybackStopped raised not only when file was ended
            //We need raise event only if playback stopped automatically
            if(IsStopManualy)
            {
                IsStopManualy = false;
                return;
            }


            var handler = PlaybackStopped;
            handler?.Invoke(this, stoppedEventArgs);
        }

        public async Task Play(MusicEntry entry)
        {
            if(CurrentSong == entry)
            {
                if(WaveOut.PlaybackState == PlaybackState.Playing)
                {
                    Pause();
                }
                else
                {
                    Play();
                }

                return;
            }

            Reader?.Dispose();

            if(CurrentSong != null)
            {
                CurrentSong.IsPlaying = false;
            }

            CurrentSong = entry;

            var dispatcher = Dispatcher.CurrentDispatcher;

            try
            {
                var stream = await CacheHelper.Download(entry);

                Reader = new Mp3FileReaderEx(stream, entry.Duration);

                dispatcher.Invoke(() =>
                {
                    WaveOut.Stop();
                    WaveOut.Init(Reader);
                    Play();
                });
            }
            catch(OperationCanceledException)
            {
                //ignore
            }
            catch(Exception e)
            {
                Trace.WriteLine($"Unable to parse file: {e}");
                throw;
            }
        }

        public void Stop()
        {
            if(WaveOut.PlaybackState != PlaybackState.Stopped)
            {
                IsStopManualy = true;
                WaveOut.Stop();

                SeekTimer.Stop();
            }

            Reader?.Dispose();

            if(CurrentSong != null)
            {
                CurrentSong.IsPlaying = false;
            }
        }

        public void Pause()
        {
            if(WaveOut.PlaybackState == PlaybackState.Playing)
            {
                WaveOut.Pause();
                CurrentSong.IsPlaying = false;

                SeekTimer.Stop();
            }
        }

        private void Play()
        {
            WaveOut.Play();
            CurrentSong.IsPlaying = true;

            SeekTimer.Start();
            OnPropertyChanged(nameof(Seek));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}