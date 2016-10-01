using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using FirstFloor.ModernUI.Windows.Controls;
using VkNet.Enums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VMM.Annotations;
using VMM.Dialog;
using VMM.Helper;
using VMM.Model;
using VMM.Player;
using Application = System.Windows.Application;

namespace VMM.Content.ViewModel
{
    internal enum PlaybackDirection
    {
        None,
        Forward,
        Backward
    }

    public class MusicListViewModel : INotifyPropertyChanged
    {
        private readonly bool _isReadOnly;
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
            MusicPlayer.Instance.PlaybackFinished += OnPlaybackFinished;
            MusicPlayer.Instance.PlaybackFailed += OnPlaybackFailed;
            MusicPlayer.Instance.EntryPlayed += OnEntryPlayed;
            _isReadOnly = SettingsVault.Read().ReadOnly;
        }

        public ICommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new DelegateCommand(Refresh));

        public ObservableCollection<MusicEntry> Music => _music ?? (_music = new ObservableCollection<MusicEntry>());

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public bool CanEdit => !_isReadOnly;

        public string BusyText
        {
            get { return _busyText; }
            set
            {
                _busyText = value;
                OnPropertyChanged(nameof(BusyText));
            }
        }

        public int ProgressMaxValue
        {
            get { return _progressMaxValue; }
            set
            {
                _progressMaxValue = value;
                OnPropertyChanged(nameof(ProgressMaxValue));
            }
        }

        public int ProgressCurrentValue
        {
            get { return _progressCurrentValue; }
            set
            {
                _progressCurrentValue = value;
                OnPropertyChanged(nameof(ProgressCurrentValue));
            }
        }


        public bool IsModified
        {
            get { return !_isReadOnly && _isModified; }
            set
            {
                _isModified = value;
                OnPropertyChanged(nameof(IsModified));
            }
        }

        private PlaybackDirection LastPlaybackDirection { get; set; } = PlaybackDirection.None;

        public MusicEntry[] SelectedItems { get; set; }

        public List<MusicListChange> ChangesList => _changesList ?? (_changesList = new List<MusicListChange>());

        public MusicEntry SelectedSong
        {
            get { return _selectedSong; }
            set
            {
                _selectedSong = value;
                OnPropertyChanged(nameof(SelectedSong));
            }
        }

        public ICommand RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<MusicEntry>(Remove));

        public ICommand SortCommand => _sortCommand ?? (_sortCommand = new DelegateCommand<MusicEntry[]>(Sort));

        public ICommand SaveChangesCommand => _saveChangesCommand ?? (_saveChangesCommand = new DelegateCommand(SaveChanges));

        public ICommand RemoveSelectedCommand => _removeSelectedCommand ?? (_removeSelectedCommand = new DelegateCommand<MusicEntry[]>(RemoveSelected));

        public ICommand SaveSelectedCommand => _saveSelectedCommand ?? (_saveSelectedCommand = new DelegateCommand<MusicEntry[]>(SaveSelected));

        public ICommand PlaySongCommand => _playSongCommand ?? (_playSongCommand = new DelegateCommand<MusicEntry>(PlayCustomSong));

        private void PlayCustomSong(MusicEntry entry)
        {
            LastPlaybackDirection = PlaybackDirection.None;
            PlaySong(entry);
        }

        public ICommand PlayNextCommand => _playNextCommand ?? (_playNextCommand = new DelegateCommand(PlayNext));

        public ICommand PlayPreviousCommand => _playPreviousCommand ?? (_playPreviousCommand = new DelegateCommand(PlayPrevious));

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPlaybackFinished(object sender, EventArgs args)
        {
            PlayNext();
        }

        private void OnPlaybackFailed(object sender, EventArgs e)
        {
            if(LastPlaybackDirection == PlaybackDirection.Backward)
            {
                PlayPrevious();
            }
            else
            {
                PlayNext();
            }
        }

        private void SaveSelected(MusicEntry[] musicEntries)
        {
            if(musicEntries == null || musicEntries.Length == 0)
            {
                return;
            }

            var dlg = new FolderBrowserDialog();
            var result = dlg.ShowDialog();

            if(result != DialogResult.OK)
            {
                return;
            }

            IsBusy = true;
            var uiDispatcher = Dispatcher.CurrentDispatcher;

            BusyText = "Подождите, выполняется сохранение выбранных песен...";
            ProgressMaxValue = musicEntries.Length;
            ProgressCurrentValue = 0;

            var savePath = dlg.SelectedPath;

            Task.Run(() =>
            {
                try
                {
                    var client = Vk.Instance.Client;

                    foreach(var song in musicEntries)
                    {
                        var fileName = $"{new string($"{song.Artist} - {song.Name}".Where(c => !"><|?*/\\:\"".Contains(c)).ToArray())}.mp3";

                        var filePath = Path.Combine(savePath, fileName);
                        if(!File.Exists(filePath))
                        {
                            lock(client)
                            {
                                client.DownloadFile(song.Url, filePath);
                            }
                        }

                        uiDispatcher.Invoke(() => { ++ProgressCurrentValue; });
                    }
                }
                catch(Exception e)
                {
                    Trace.WriteLine($"While saving file: {e}");

                    uiDispatcher.Invoke(() => { ModernDialog.ShowMessage("Во время сохранения произошла ошибка :(", "Не удалось сохранить файл", MessageBoxButton.OK); });
                }
                finally
                {
                    uiDispatcher.Invoke(() => { IsBusy = false; });
                }
            });
        }

        private void RemoveSelected(MusicEntry[] musicEntries)
        {
            foreach(var entry in musicEntries)
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

            var uiDispatcher = Dispatcher.CurrentDispatcher;

            BusyText = "Подождите, обновляется список музыки...";
            ProgressMaxValue = 0;
            ProgressCurrentValue = 0;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var albums = Vk.Instance.Api.Audio.GetAlbums(Vk.Instance.UserId);
                    User user = null;
                    var musicList = Vk.Instance.Api.Audio.Get(out user, new AudioGetParams());
                    long surrogateIndexes = long.MaxValue;

                    foreach(var musicEntry in musicList)
                    {
                        var song = musicEntry;

                        var entry = new MusicEntry
                        {
                            Id = song.Id ?? --surrogateIndexes,
                            Artist = song.Artist,
                            Name = song.Title,
                            Genre = song.Genre ?? AudioGenre.Other,
                            Duration = song.Duration,
                            Url = song.Url
                        };

                        if(song.AlbumId.HasValue)
                        {
                            entry.Album = albums.SingleOrDefault(c => c.AlbumId == song.AlbumId.Value);
                        }

                        uiDispatcher.BeginInvoke(new Action(() => Music.Add(entry)));
                    }
                }
                finally
                {
                    uiDispatcher.Invoke(() => { IsBusy = false; });
                }
            });
        }


        private void Remove(MusicEntry song)
        {
            if(IsBusy)
            {
                return;
            }

            song.IsDeleted = !song.IsDeleted;

            if(song.IsDeleted)
            {
                ChangesList.Add(new MusicListChange {ChangeType = ChangeType.Deleted, Data = new DeleteSong {SongId = (ulong)song.Id}});
                IsModified = true;
            }
            else
            {
                ChangesList.RemoveAll(c => c.ChangeType == ChangeType.Deleted && ((DeleteSong)c.Data).SongId == (ulong)song.Id);
            }
        }

        private void Sort(MusicEntry[] selectedItems)
        {
            var dlg = new SortSettings {Owner = Application.Current.MainWindow};
            var result = dlg.ShowDialog();

            if(result != true)
            {
                return;
            }

            var sortingPaths = dlg.SortingPaths;


            var musicEntries = Music.ToList();
            var selectedEntries = (MusicEntry[])selectedItems.Clone();
            Music.Clear();

            IsBusy = true;
            var uiDispatcher = Dispatcher.CurrentDispatcher;

            BusyText = "Подождите, выполняется сортировка...";
            ProgressMaxValue = 0;
            ProgressCurrentValue = 0;

            Task.Run(() =>
            {
                var itemsToSort = selectedEntries.Length > 1 ? selectedEntries : musicEntries.ToArray();
                var startPosition = musicEntries.IndexOf(itemsToSort.First());
                musicEntries.RemoveAll(itemsToSort.Contains);

                var sorted = SortHelper.Sort(itemsToSort, sortingPaths);

                musicEntries.InsertRange(startPosition, sorted);

                foreach(var musicEntry in musicEntries)
                {
                    var entry = musicEntry;
                    uiDispatcher.BeginInvoke(new Action(() => Music.Add(entry)));
                }

                uiDispatcher.Invoke(() => { IsBusy = false; });
            });

            IsModified = true;
        }

        private void SaveChanges()
        {
            IsBusy = true;
            var uiDispatcher = Dispatcher.CurrentDispatcher;

            BusyText = "Подождите, применяются изменения...";
            ProgressMaxValue = Music.Count + ChangesList.Count - 1;
            ProgressCurrentValue = 0;

            Task.Run(() =>
            {
                try
                {
                    //TODO: Optimize reorder requests
                    for(var i = 0; i < Music.Count; i++)
                    {
                        var entry = Music[i];
                        var previousId = i != 0 ? Music[i - 1].Id : 0;
                        var nextId = i < Music.Count - 1 ? Music[i + 1].Id : 0;

                        Vk.Instance.Api.Audio.Reorder(entry.Id, Vk.Instance.UserId, (long)previousId, (long)nextId);

                        uiDispatcher.BeginInvoke(new Action(() => { ++ProgressCurrentValue; }));

                        Thread.Sleep(340); //Allowed only 3 request per second
                    }

                    foreach(var change in ChangesList.Where(c => c.ChangeType == ChangeType.Deleted))
                    {
                        Vk.Instance.Api.Audio.Delete((long)((DeleteSong)change.Data).SongId, Vk.Instance.UserId);

                        uiDispatcher.BeginInvoke(new Action(() => { ++ProgressCurrentValue; }));

                        Thread.Sleep(340);
                    }
                }
                finally
                {
                    uiDispatcher.Invoke(() => { IsBusy = false; });
                    uiDispatcher.BeginInvoke(new Action(Refresh));
                }
            });
        }

        private void PlaySong(MusicEntry musicEntry)
        {
            if(musicEntry == null)
            {
                musicEntry = Music.FirstOrDefault();
            }

            if(musicEntry != null)
            {
                MusicPlayer.Instance.Play(musicEntry);
            }
        }

        private void PlayNext()
        {
            var current = MusicPlayer.Instance.CurrentSong;

            var currentIndex = Music.IndexOf(current);
            MusicEntry next = null;
            if(currentIndex >= 0 && currentIndex < Music.Count - 1)
            {
                next = Music[currentIndex + 1];
            }
            else if(currentIndex == Music.Count - 1)
            {
                next = Music.FirstOrDefault();
            }

            if(next != null)
            {
                LastPlaybackDirection = PlaybackDirection.Forward;
                PlaySong(next);
            }
        }

        private void PlayPrevious()
        {
            var current = MusicPlayer.Instance.CurrentSong;

            var currentIndex = Music.IndexOf(current);
            MusicEntry previous = null;
            if(currentIndex > 0 && currentIndex < Music.Count)
            {
                previous = Music[currentIndex - 1];
            }
            else if(currentIndex == 0)
            {
                previous = Music.LastOrDefault();
            }

            if(previous != null)
            {
                LastPlaybackDirection = PlaybackDirection.Backward;
                PlaySong(previous);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void MoveSong(int srcIndex, int targetIndex)
        {
            if(srcIndex != targetIndex)
            {
                Music.Move(srcIndex, targetIndex);
                IsModified = true;
            }
        }

        public event EventHandler<MusicEntry> EntryPlayed;

        protected virtual void OnEntryPlayed(object sender, MusicEntry musicEntry)
        {
            EntryPlayed?.Invoke(sender, musicEntry);
        }
    }
}