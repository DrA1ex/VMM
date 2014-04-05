using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using VkNet.Enums;
using VkNet.Model;
using VMM.Annotations;
using VMM.Helper;
using VMM.Model;

namespace VMM.Content.ViewModel
{
    public class MusicListViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        private ObservableCollection<MusicEntry> _music;
        private ICommand _refreshCommand;

        public ICommand RefreshCommand
        {
            get { return _refreshCommand ?? (_refreshCommand = new DelegateCommand(Refresh)); }
        }

        public ObservableCollection<MusicEntry> Music
        {
            get { return _music ?? (_music = new ObservableCollection<MusicEntry>()); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Refresh()
        {
            IsBusy = true;
            Music.Clear();

            Dispatcher disp = Dispatcher.CurrentDispatcher;

            Task.Factory.StartNew(() =>
                                  {
                                      try
                                      {
                                          ReadOnlyCollection<Audio> musicList = Vk.Instance.Api.Audio.Get(Vk.Instance.UserId);

                                          foreach (Audio musicEntry in musicList)
                                          {
                                              Audio song = musicEntry;
                                              disp.BeginInvoke(new Action(() => Music.Add(new MusicEntry
                                                                                          {
                                                                                              Id = song.Id,
                                                                                              Artist = song.Artist,
                                                                                              Name = song.Title,
                                                                                              Genre = song.Genre ?? AudioGenre.Other,
                                                                                              Duration = song.Duration,
                                                                                              Url = song.Url,
                                                                                              AlbumId = song.AlbumId
                                                                                          })));
                                          }
                                      }
                                      finally
                                      {
                                          disp.Invoke(() => { IsBusy = false; });
                                      }
                                  });
        }


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