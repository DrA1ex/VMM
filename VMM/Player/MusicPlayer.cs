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
        private double _buffered;
        private MusicEntry _currentSong;

        private MusicPlayer()
        {
            Engine.PlaybackFinished += OnPlaybackFinished;
            Engine.PlaybackFailed += (s, e) => OnPlaybackFinished(s, EventArgs.Empty);
            Engine.Buffred += (sender, d) => Buffered = d;

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

        public double Buffered
        {
            get { return _buffered; }
            set
            {
                _buffered = value;
                OnPropertyChanged();
            }
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
            if(CurrentSong.IsPlaying)
            {
                OnEntryPlayed(CurrentSong);
            }
        }

        ~MusicPlayer()
        {
            Dispose();
        }

        public event EventHandler PlaybackFinished;
        public event EventHandler<MusicEntry> EntryPlayed;

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

        protected virtual void OnEntryPlayed(MusicEntry e)
        {
            EntryPlayed?.Invoke(this, e);
        }
    }
}