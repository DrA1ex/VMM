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
        public ulong Id { get; set; }

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
                OnPropertyChanged(nameof(IsDeleted));
            }
        }


        public bool Modified
        {
            get { return _modified; }
            set
            {
                _modified = value;
                OnPropertyChanged(nameof(Modified));
            }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class MusicEntryDesign
    {
        public long Id => 123456;

        public string Artist => "Three Days Grace";

        public string Name => "Never Too Late";

        public AudioGenre Genre => AudioGenre.Rock;

        public AudioAlbum Album => new AudioAlbum {Title = "One-X"};

        public int Duration => 3 * 60 + 31;

        public Uri Url => new Uri("http:\\examle.com");

        public bool IsDeleted => false;

        public bool IsPlaying => false;

        public bool IsLoading => true;
    }
}