using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using JetBrains.Annotations;
using Microsoft.Win32;
using VMM.Helper;
using VMM.Model;

namespace VMM.Content.ViewModel
{
    public class MusicEntryViewModel : INotifyPropertyChanged
    {
        private ICommand _saveSongCommand;

        private bool _savingInProgress;

        public ICommand SaveSongCommand
        {
            get { return _saveSongCommand ?? (_saveSongCommand = new DelegateCommand<MusicEntry>(SaveSong)); }
        }

        public bool SavingInProgress
        {
            get { return _savingInProgress; }
            set
            {
                _savingInProgress = value;
                OnPropertyChanged("SavingInProgress");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SaveSong(MusicEntry song)
        {
            var dlg = new SaveFileDialog
                      {
                          FileName = String.Format("{0}.mp3",
                              new String(String.Format("{0} - {1}", song.Artist, song.Name).Where(c => !"><|?*/\\:\"".Contains(c)).ToArray())),
                          Filter = "Файл mp3|*.mp3"
                      };

            if (dlg.ShowDialog(Application.Current.MainWindow) == true)
            {
                SavingInProgress = true;
                Dispatcher disp = Dispatcher.CurrentDispatcher;

                Task.Run(() =>
                         {
                             try
                             {
                                 var client = Vk.Instance.Client;
                                 lock (client)
                                 {
                                     client.DownloadFile(song.Url, dlg.FileName);
                                 }
                             }
                             catch (Exception e)
                             {
                                 Trace.WriteLine(String.Format("While saving file: {0}", e));
                             }
                             finally
                             {
                                 disp.Invoke(() => { SavingInProgress = false; });
                             }
                         });
            }
        }

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