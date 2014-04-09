using System;
using System.ComponentModel;
using VkNet.Enums;
using VkNet.Model;
using VMM.Annotations;

namespace VMM.Model
{
    public class MusicEntry : INotifyPropertyChanged
    {
        private bool _isDeleted;
        private bool _isLoading;
        private bool _isPlaying;
        private bool _modified;
        public long Id { get; set; }

        public string Artist { get; set; }

        public string Name { get; set; }

        public AudioGenre Genre { get; set; }

        public AudioAlbum Album { get; set; }

        public int Duration { get; set; }

        public Uri Url { get; set; }


        public bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                _isDeleted = value;
                OnPropertyChanged("IsDeleted");
            }
        }


        public bool Modified
        {
            get { return _modified; }
            set
            {
                _modified = value;
                OnPropertyChanged("Modified");
            }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                OnPropertyChanged("IsPlaying");
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    internal class MusicEntryDesign
    {
        public long Id
        {
            get { return 123456; }
        }

        public string Artist
        {
            get { return "Three Days Grace"; }
        }

        public string Name
        {
            get { return "Never Too Late"; }
        }

        public AudioGenre Genre
        {
            get { return AudioGenre.Rock; }
        }

        public AudioAlbum Album
        {
            get { return new AudioAlbum { Title = "One-X" }; }
        }

        public int Duration
        {
            get { return 3 * 60 + 31; }
        }

        public Uri Url
        {
            get { return new Uri("http:\\examle.com"); }
        }

        public bool IsDeleted
        {
            get { return false; }
        }

        public bool IsPlaying
        {
            get { return false; }
        }

        public bool IsLoading
        {
            get { return true; }
        }
    }
}