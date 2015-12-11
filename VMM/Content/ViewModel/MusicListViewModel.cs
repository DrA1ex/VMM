using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using FirstFloor.ModernUI.Windows.Controls;
using VkNet.Enums;
using VkNet.Model;
using VkNet.Model.Attachments;
using VMM.Annotations;
using VMM.Dialog;
using VMM.Helper;
using VMM.Model;
using VMM.Utils;
using Application = System.Windows.Application;

namespace VMM.Content.ViewModel
{
    public class MusicListViewModel : INotifyPropertyChanged
    {
        private string _busyText;
        private List<MusicListChange> _changesList;
        private bool _isBusy;
        private bool _isModified;
        private ObservableCollection<MusicEntry> _music;
        private ICommand _playNextCommand;
        private ICommand _playPreviousCommand;
        private ICommand _playSongCommand;
        private int _progressCurrentValue;
        private int _progressMaxValue;
        private ICommand _refreshCommand;
        private ICommand _removeCommand;
        private ICommand _removeSelectedCommand;
        private ICommand _saveChangesCommand;
        private ICommand _saveSelectedCommand;
        private MusicEntry _selectedSong;
        private ICommand _sortCommand;

        public MusicListViewModel()
        {
            MusicPlayer.Instance.PlaybackStopped += delegate { PlayNext(); };
        }


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

        public string BusyText
        {
            get { return _busyText; }
            set
            {
                _busyText = value;
                OnPropertyChanged("BusyText");
            }
        }

        public int ProgressMaxValue
        {
            get { return _progressMaxValue; }
            set
            {
                _progressMaxValue = value;
                OnPropertyChanged("ProgressMaxValue");
            }
        }

        public int ProgressCurrentValue
        {
            get { return _progressCurrentValue; }
            set
            {
                _progressCurrentValue = value;
                OnPropertyChanged("ProgressCurrentValue");
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

        public MusicEntry[] SelectedItems { get; set; }

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

        public ICommand RemoveSelectedCommand
        {
            get { return _removeSelectedCommand ?? (_removeSelectedCommand = new DelegateCommand<MusicEntry[]>(RemoveSelected)); }
        }

        public ICommand SaveSelectedCommand
        {
            get { return _saveSelectedCommand ?? (_saveSelectedCommand = new DelegateCommand<MusicEntry[]>(SaveSelected)); }
        }

        public ICommand PlaySongCommand
        {
            get { return _playSongCommand ?? (_playSongCommand = new DelegateCommand<MusicEntry>(PlaySong)); }
        }

        public ICommand PlayNextCommand
        {
            get { return _playNextCommand ?? (_playNextCommand = new DelegateCommand(PlayNext)); }
        }

        public ICommand PlayPreviousCommand
        {
            get { return _playPreviousCommand ?? (_playPreviousCommand = new DelegateCommand(PlayPrevious)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SaveSelected(MusicEntry[] musicEntries)
        {
            if (musicEntries == null || musicEntries.Length == 0)
            {
                return;
            }

            var dlg = new FolderBrowserDialog();
            DialogResult result = dlg.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            IsBusy = true;
            Dispatcher disp = Dispatcher.CurrentDispatcher;

            BusyText = "Подождите, выполняется сохранение выбранных песен...";
            ProgressMaxValue = musicEntries.Length;
            ProgressCurrentValue = 0;

            string savePath = dlg.SelectedPath;

            Task.Run(() =>
                     {
                         try
                         {
                             WebClient client = Vk.Instance.Client;

                             foreach (MusicEntry song in musicEntries)
                             {
                                 string fileName = String.Format("{0}.mp3",
                                     new String(String.Format("{0} - {1}", song.Artist, song.Name).Where(c => !"><|?*/\\:\"".Contains(c)).ToArray()));

                                 string filePath = Path.Combine(savePath, fileName);
                                 if (!File.Exists(filePath))
                                 {
                                     lock (client)
                                     {
                                         client.DownloadFile(song.Url, filePath);
                                     }
                                 }

                                 disp.Invoke(() => { ++ProgressCurrentValue; });
                             }
                         }
                         catch (Exception e)
                         {
                             Trace.WriteLine(String.Format("While saving file: {0}", e));

                             disp.Invoke(() => { ModernDialog.ShowMessage("Во время сохранения произошла ошибка :(", "Не удалось сохранить файл", MessageBoxButton.OK); });
                         }
                         finally
                         {
                             disp.Invoke(() => { IsBusy = false; });
                         }
                     });
        }

        private void RemoveSelected(MusicEntry[] musicEntries)
        {
            foreach (MusicEntry entry in musicEntries)
            {
                entry.IsDeleted = true;
            }
        }

        public void Refresh()
        {
            IsBusy = true;
            Music.Clear();
            ChangesList.Clear();
            IsModified = false;

            Dispatcher disp = Dispatcher.CurrentDispatcher;

            BusyText = "Подождите, обновляется список музыки...";
            ProgressMaxValue = 0;
            ProgressCurrentValue = 0;

            Task.Factory.StartNew(() =>
                                  {
                                      try
                                      {
                                          ReadOnlyCollection<AudioAlbum> albums = Vk.Instance.Api.Audio.GetAlbums(Vk.Instance.UserId);
                                          ReadOnlyCollection<Audio> musicList = Vk.Instance.Api.Audio.Get((ulong)Vk.Instance.UserId);

                                          foreach (Audio musicEntry in musicList)
                                          {
                                              Audio song = musicEntry;

                                              var entry = new MusicEntry
                                                          {
                                                              Id = (ulong)song.Id,
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
            var dlg = new SortSettings { Owner = Application.Current.MainWindow };
            bool? result = dlg.ShowDialog();

            if (result != true)
            {
                return;
            }

            SortingPath[] sortingPaths = dlg.SortingPaths;


            List<MusicEntry> musicEntries = Music.ToList();
            var selectedEntries = (MusicEntry[])selectedItems.Clone();
            Music.Clear();

            IsBusy = true;
            Dispatcher disp = Dispatcher.CurrentDispatcher;

            BusyText = "Подождите, выполняется сортировка...";
            ProgressMaxValue = 0;
            ProgressCurrentValue = 0;

            Task.Run(() =>
                     {
                         MusicEntry[] itemsToSort = selectedEntries.Length > 1 ? selectedEntries : musicEntries.ToArray();
                         int startPosition = musicEntries.IndexOf(itemsToSort.First());
                         musicEntries.RemoveAll(itemsToSort.Contains);

                         IEnumerable<MusicEntry> sorted = SortHelper.Sort(itemsToSort, sortingPaths);

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

            BusyText = "Подождите, применяются изменения...";
            ProgressMaxValue = Music.Count + ChangesList.Count - 1;
            ProgressCurrentValue = 0;

            Task.Run(() =>
                     {
                         try
                         {
                             //TODO: Optimize reorder requests
                             for (int i = 0; i < Music.Count; i++)
                             {
                                 MusicEntry entry = Music[i];
                                 ulong previosId = i != 0 ? Music[i - 1].Id : 0;
                                 ulong nextId = i < Music.Count - 1 ? Music[i + 1].Id : 0;

                                 Vk.Instance.Api.Audio.Reorder(entry.Id, Vk.Instance.UserId, (long)previosId, (long)nextId);

                                 disp.BeginInvoke(new Action(() => { ++ProgressCurrentValue; }));

                                 Thread.Sleep(340); //Allowed only 3 request per second
                             }

                             foreach (MusicListChange change in ChangesList.Where(c => c.ChangeType == ChangeType.Deleted))
                             {
                                 Vk.Instance.Api.Audio.Delete(((DeleteSong)change.Data).SongId, Vk.Instance.UserId);

                                 disp.BeginInvoke(new Action(() => { ++ProgressCurrentValue; }));

                                 Thread.Sleep(340);
                             }
                         }
                         finally
                         {
                             disp.Invoke(() => { IsBusy = false; });
                             disp.BeginInvoke(new Action(Refresh));
                         }
                     });
        }

        private void PlaySong(MusicEntry musicEntry)
        {
            if (musicEntry == null)
            {
                musicEntry = Music.FirstOrDefault();
            }

            if (MusicPlayer.Instance.CurrentSong != null && MusicPlayer.Instance.CurrentSong != musicEntry)
                MusicPlayer.Instance.Stop();

            if (musicEntry != null)
            {
                Task.Run(() =>
                         {
                             lock (MusicPlayer.Instance)
                                 MusicPlayer.Instance.Play(musicEntry);
                         });
            }
        }

        private void PlayNext()
        {
            if (MusicPlayer.Instance.CurrentSong != null)
                MusicPlayer.Instance.Stop();

            MusicEntry current = MusicPlayer.Instance.CurrentSong;

            int currentIndex = Music.IndexOf(current);
            MusicEntry next = null;
            if (currentIndex >= 0 && currentIndex < Music.Count - 1)
            {
                next = Music[currentIndex + 1];
            }
            else if (currentIndex == Music.Count - 1)
            {
                next = Music.FirstOrDefault();
            }

            if (next != null)
            {
                PlaySong(next);
            }
        }

        private void PlayPrevious()
        {
            if (MusicPlayer.Instance.CurrentSong != null)
                MusicPlayer.Instance.Stop();

            MusicEntry current = MusicPlayer.Instance.CurrentSong;

            int currentIndex = Music.IndexOf(current);
            MusicEntry previous = null;
            if (currentIndex > 0 && currentIndex < Music.Count)
            {
                previous = Music[currentIndex - 1];
            }
            else if (currentIndex == 0)
            {
                previous = Music.LastOrDefault();
            }

            if (previous != null)
            {
                PlaySong(previous);
            }
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