using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace VMM.Model
{
    public class SortingPath : INotifyPropertyChanged
    {
        private bool _descending;

        #region Predefined SortingPaths
        public static SortingPath Artist
        {
            get
            {
                return new SortingPath
                       {
                           Expression = entry => entry.Artist,
                           DisplayName = "Исполнитель"
                       };
            }
        }

        public static SortingPath Name
        {
            get
            {
                return new SortingPath
                       {
                           Expression = entry => entry.Name,
                           DisplayName = "Название"
                       };
            }
        }

        public static SortingPath Album
        {
            get
            {
                return new SortingPath
                       {
                           Expression = entry => entry.Album != null ? entry.Album.Title : null,
                           DisplayName = "Альбом"
                       };
            }
        }

        public static SortingPath Genre
        {
            get
            {
                return new SortingPath
                       {
                           Expression = entry => (int)entry.Genre,
                           DisplayName = "Жанр"
                       };
            }
        }
        #endregion

        public Func<MusicEntry, object> Expression { get; set; }

        public string DisplayName { get; set; }

        public bool Descending
        {
            get { return _descending; }
            set
            {
                _descending = value;
                OnPropertyChanged("Descending");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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