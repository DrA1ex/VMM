using System;
using System.IO;
using NAudio.Wave;
using VMM.Helper;
using VMM.Model;

namespace VMM.Utils
{
    public class MusicPlayer : IDisposable
    {
        static MusicPlayer()
        {
            Instance = new MusicPlayer();
        }

        private MusicPlayer()
        {
            WaveOut = new WaveOut();
            WaveOut.PlaybackStopped += OnPlaybackStopped;
        }


        public static MusicPlayer Instance { get; private set; }

        private WaveOut WaveOut { get; set; }
        private Mp3FileReader Reader { get; set; }
        public MusicEntry CurrentSong { get; private set; }

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
            }

            if (CurrentSong != null)
                CurrentSong.IsPlaying = false;
        }

        public void Pause()
        {
            if (WaveOut.PlaybackState == PlaybackState.Playing)
            {
                WaveOut.Pause();
                CurrentSong.IsPlaying = false;
            }
        }

        private void Play()
        {
            WaveOut.Play();
            CurrentSong.IsPlaying = true;
        }
    }
}