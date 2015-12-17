using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using JetBrains.Annotations;
using NAudio.Wave;
using VMM.Model;

namespace VMM.Player
{
    public class MusicPlayer : IDisposable, INotifyPropertyChanged
    {
        private MusicEntry _currentSong;

        private MusicPlayer()
        {
            Engine.PlaybackFinished += OnPlaybackFinished;
            Engine.PlaybackFailed += (s, e) => OnPlaybackFinished(s, EventArgs.Empty);

            Engine.PlaybackStateChanged += EngineOnPlaybackStateChanged;

            SeekTimer.Interval = TimeSpan.FromSeconds(0.33);
            SeekTimer.Tick += (sender, args) => OnPropertyChanged(nameof(Seek));
        }

        public static MusicPlayer Instance { get; } = new MusicPlayer();
        private MusicPlayerEngine Engine { get; } = new MusicPlayerEngine();

        public DispatcherTimer SeekTimer { get; } = new DispatcherTimer();

        public double Volume
        {
            get { return Engine.Volume; }
            set { Engine.Volume = (float)value; }
        }

        public double Seek
        {
            get { return Engine.Seek; }
            set { Engine.Seek = value; }
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Engine.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void EngineOnPlaybackStateChanged(object sender, PlaybackState playbackState)
        {
            CurrentSong.IsPlaying = playbackState == PlaybackState.Playing;
        }

        ~MusicPlayer()
        {
            Dispose();
        }

        public event EventHandler PlaybackFinished;

        protected virtual void OnPlaybackFinished(object sender, EventArgs args)
        {
            var handler = PlaybackFinished;
            handler?.Invoke(this, args);
        }

        public void Play(MusicEntry entry)
        {
            if(CurrentSong == entry)
            {
                Engine.PlayPause();
                return;
            }

            if(CurrentSong != null)
            {
                CurrentSong.IsPlaying = false;
            }

            Engine.Play(entry);
            CurrentSong = entry;

            SeekTimer.Start();
        }

        public void Stop()
        {
            Engine.Stop();
            SeekTimer.Stop();
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}