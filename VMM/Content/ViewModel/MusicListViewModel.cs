using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
        private List<MusicListChange> _changesList;
        private bool _isBusy;
        private bool _isModified;
        private ICommand _moveDownCommand;
        private ICommand _moveUpCommand;
        private ObservableCollection<MusicEntry> _music;
        private ICommand _refreshCommand;
        private ICommand _removeCommand;
        private ICommand _saveChangesCommand;
        private MusicEntry _selectedSong;
        private ICommand _sortCommand;

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


        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                _isModified = value;
                OnPropertyChanged("IsModified");
            }
        }

        public List<MusicListChange> ChangesList
        {
            get { return _changesList ?? (_changesList = new List<MusicListChange>()); }
        }

        public MusicEntry SelectedSong
        {
            get { return _selectedSong; }
            set
            {
                _selectedSong = value;
                OnPropertyChanged("SelectedSong");
            }
        }

        public ICommand MoveUpCommand
        {
            get { return _moveUpCommand ?? (_moveUpCommand = new DelegateCommand(MoveUp)); }
        }

        public ICommand MoveDownCommand
        {
            get { return _moveDownCommand ?? (_moveDownCommand = new DelegateCommand(MoveDown)); }
        }

        public ICommand RemoveCommand
        {
            get { return _removeCommand ?? (_removeCommand = new DelegateCommand<MusicEntry>(Remove)); }
        }

        public ICommand SortCommand
        {
            get { return _sortCommand ?? (_sortCommand = new DelegateCommand(Sort)); }
        }

        public ICommand SaveChangesCommand
        {
            get { return _saveChangesCommand ?? (_saveChangesCommand = new DelegateCommand(SaveChanges)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Refresh()
        {
            IsBusy = true;
            Music.Clear();
            ChangesList.Clear();
            IsModified = false;

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


        private void MoveUp()
        {
            if (SelectedSong != null)
            {
                int index = Music.IndexOf(SelectedSong);
                int newPosition = index - 1;

                if (index != 0)
                {
                    Music.Move(index, newPosition);
                    ChangesList.Add(new MusicListChange { ChangeType = ChangeType.Moved, Data = new MovedSong { SongId = SelectedSong.Id, Position = newPosition } });

                    IsModified = true;
                }
            }
        }

        private void MoveDown()
        {
            if (SelectedSong != null)
            {
                int index = Music.IndexOf(SelectedSong);
                int newPosition = index + 1;

                if (index < Music.Count - 1)
                {
                    Music.Move(index, newPosition);
                    ChangesList.Add(new MusicListChange { ChangeType = ChangeType.Moved, Data = new MovedSong { SongId = SelectedSong.Id, Position = newPosition } });

                    IsModified = true;
                }
            }
        }

        private void Remove(MusicEntry song)
        {
            song.IsDeleted = !song.IsDeleted;

            if (song.IsDeleted)
            {
                ChangesList.Add(new MusicListChange { ChangeType = ChangeType.Deleted, Data = new DeleteSong { SongId = song.Id } });
                IsModified = true;
            }
            else
            {
                ChangesList.RemoveAll(c => c.ChangeType == ChangeType.Deleted && c.Data is DeleteSong && ((DeleteSong)c.Data).SongId == song.Id);

                IsModified = ChangesList.Any();
            }
        }

        private void Sort()
        {
            MessageBox.Show("Not implemented");

            IsModified = ChangesList.Any();
        }

        private void SaveChanges()
        {
            MessageBox.Show("Not implemented");

            IsModified = false;
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