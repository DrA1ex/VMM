using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using JetBrains.Annotations;
using NAudio.Utils;
using NAudio.Wave;
using VMM.Helper;
using VMM.Model;

namespace VMM.Utils
{
    public class MusicPlayer : IDisposable, INotifyPropertyChanged
    {
        private MusicEntry _currentSong;

        static MusicPlayer()
        {
            Instance = new MusicPlayer();
        }

        private MusicPlayer()
        {
            WaveOut = new WaveOut();
            WaveOut.PlaybackStopped += OnPlaybackStopped;

            SeekTimer.Interval = TimeSpan.FromSeconds(0.33);
            SeekTimer.Tick += (sender, args) => OnPropertyChanged("Seek");
        }


        public static MusicPlayer Instance { get; private set; }

        private WaveOut WaveOut { get; set; }
        private Mp3FileReader Reader { get; set; }

        private DispatcherTimer _seekTimer;

        public DispatcherTimer SeekTimer
        {
            get { return _seekTimer ?? (_seekTimer = new DispatcherTimer()); }
        }

        public double Volume
        {
            get { return WaveOut.Volume; }
            set { WaveOut.Volume = (float)value; }
        }

        public double Seek
        {
            get
            {
                return Reader != null && WaveOut.PlaybackState != PlaybackState.Stopped ? (double)Reader.Position / Reader.Length : 0.0;
            }
            set
            {
                if (Reader != null)
                    Reader.Seek((long)(value * Reader.Length), SeekOrigin.Begin);
            }
        }

        public MusicEntry CurrentSong
        {
            get { return _currentSong; }
            set
            {
                _currentSong = value;
                OnPropertyChanged("CurrentSong");
            }
        }

        private bool IsStopManualy { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (Reader != null)
            {
                Reader.Dispose();
            }
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
            if (IsStopManualy)
            {
                IsStopManualy = false;
                return;
            }


            EventHandler<StoppedEventArgs> handler = PlaybackStopped;
            if (handler != null)
            {
                handler(this, stoppedEventArgs);
            }
        }

        public void Play(MusicEntry entry)
        {
            if (CurrentSong == entry)
            {
                if (WaveOut.PlaybackState == PlaybackState.Playing)
                {
                    Pause();
                }
                else
                {
                    Play();
                }

                return;
            }

            if (Reader != null)
            {
                Stop();
                Reader.Dispose();
            }

            CurrentSong = entry;
            MemoryStream stream = CacheHelper.Download(entry);
            Reader = new Mp3FileReader(stream);

            WaveOut.Init(Reader);
            Play();
        }

        public void Stop()
        {
            if (WaveOut.PlaybackState != PlaybackState.Stopped)
            {
                IsStopManualy = true;
                WaveOut.Stop();

                SeekTimer.Stop();
            }

            if (CurrentSong != null)
            {
                CurrentSong.IsPlaying = false;
            }
        }

        public void Pause()
        {
            if (WaveOut.PlaybackState == PlaybackState.Playing)
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
            OnPropertyChanged("Seek");
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}