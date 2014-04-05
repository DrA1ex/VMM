﻿using System;
using System.ComponentModel;
using VkNet.Enums;
using VMM.Annotations;

namespace VMM.Model
{
    public class MusicEntry : INotifyPropertyChanged
    {
        private bool _modified;
        public long Id { get; set; }

        public string Artist { get; set; }

        public string Name { get; set; }

        public AudioGenre Genre { get; set; }

        public long? AlbumId { get; set; }

        public int Duration { get; set; }

        public Uri Url { get; set; }


        public bool Modified
        {
            get { return _modified; }
            set
            {
                _modified = value;
                OnPropertyChanged("Modified");
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
}