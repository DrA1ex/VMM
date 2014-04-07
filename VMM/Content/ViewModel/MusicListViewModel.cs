using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VkNet.Enums;
using VkNet.Model;
using VMM.Annotations;
using VMM.Dialog;
using VMM.Helper;
using VMM.Model;

namespace VMM.Content.ViewModel
{
    public class MusicListViewModel : INotifyPropertyChanged
    {
        private List<MusicListChange> _changesList;
        private bool _isBusy;
        private bool _isModified;
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

        public ICommand RemoveCommand
        {
            get { return _removeCommand ?? (_removeCommand = new DelegateCommand<MusicEntry>(Remove)); }
        }

        public ICommand SortCommand
        {
            get { return _sortCommand ?? (_sortCommand = new DelegateCommand<MusicEntry[]>(Sort)); }
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
                                          ReadOnlyCollection<AudioAlbum> albums = Vk.Instance.Api.Audio.GetAlbums(Vk.Instance.UserId);
                                          ReadOnlyCollection<Audio> musicList = Vk.Instance.Api.Audio.Get(Vk.Instance.UserId);

                                          foreach (Audio musicEntry in musicList)
                                          {
                                              Audio song = musicEntry;

                                              var entry = new MusicEntry
                                                          {
                                                              Id = song.Id,
                                                              Artist = song.Artist,
                                                              Name = song.Title,
                                                              Genre = song.Genre ?? AudioGenre.Other,
                                                              Duration = song.Duration,
                                                              Url = song.Url,
                                                          };

                                              if (song.AlbumId.HasValue)
                                              {
                                                  entry.Album = albums.Single(c => c.AlbumId == song.AlbumId.Value);
                                              }

                                              disp.BeginInvoke(new Action(() => Music.Add(entry)));
                                          }
                                      }
                                      finally
                                      {
                                          disp.Invoke(() => { IsBusy = false; });
                                      }
                                  });
        }


        private void Remove(MusicEntry song)
        {
            if (IsBusy)
            {
                return;
            }


            song.IsDeleted = !song.IsDeleted;

            if (song.IsDeleted)
            {
                ChangesList.Add(new MusicListChange { ChangeType = ChangeType.Deleted, Data = new DeleteSong { SongId = song.Id } });
                IsModified = true;
            }
            else
            {
                ChangesList.RemoveAll(c => c.ChangeType == ChangeType.Deleted && ((DeleteSong)c.Data).SongId == song.Id);
            }
        }

        private void Sort(MusicEntry[] selectedItems)
        {
            var dlg = new SortSettings();
            dlg.Owner = Application.Current.MainWindow;
            var result = dlg.ShowDialog();

            if (result != true)
                return;

            var sortingPaths = dlg.SortingPaths;


            List<MusicEntry> musicEntries = Music.ToList();
            var selectedEntries = (MusicEntry[])selectedItems.Clone();
            Music.Clear();

            IsBusy = true;
            Dispatcher disp = Dispatcher.CurrentDispatcher;

            Task.Run(() =>
                     {
                         MusicEntry[] itemsToSort = selectedEntries.Length > 1 ? selectedEntries : musicEntries.ToArray();
                         int startPosition = musicEntries.IndexOf(itemsToSort.First());
                         musicEntries.RemoveAll(itemsToSort.Contains);

                         var sorted = SortHelper.Sort(itemsToSort, sortingPaths);

                         musicEntries.InsertRange(startPosition, sorted);

                         foreach (MusicEntry musicEntry in musicEntries)
                         {
                             MusicEntry entry = musicEntry;
                             disp.BeginInvoke(new Action(() => Music.Add(entry)));
                         }

                         disp.Invoke(() => { IsBusy = false; });
                     });

            IsModified = true;
        }

        private void SaveChanges()
        {
            IsBusy = true;
            Dispatcher disp = Dispatcher.CurrentDispatcher;

            Task.Run(() =>
                     {
                         try
                         {
                             //TODO: Optimize reorder requests
                             for (int i = 0; i < Music.Count; i++)
                             {
                                 MusicEntry entry = Music[i];
                                 long previosId = i != 0 ? Music[i - 1].Id : 0;
                                 long nextId = i < Music.Count - 1 ? Music[i + 1].Id : 0;

                                 Vk.Instance.Api.Audio.Reorder(entry.Id, Vk.Instance.UserId, previosId, nextId);
                                 Thread.Sleep(340); //Allowed only 3 request per second
                             }

                             foreach (MusicListChange change in ChangesList.Where(c => c.ChangeType == ChangeType.Deleted))
                             {
                                 Vk.Instance.Api.Audio.Delete(((DeleteSong)change.Data).SongId, Vk.Instance.UserId);
                             }
                         }
                         finally
                         {
                             disp.Invoke(() => { IsBusy = false; });
                             disp.BeginInvoke(new Action(Refresh));
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

        public void MoveSong(int srcIndex, int targetIndex)
        {
            if (srcIndex != targetIndex)
            {
                Music.Move(srcIndex, targetIndex);
                IsModified = true;
            }
        }
    }
}