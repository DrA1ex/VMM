using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using VMM.Content.ViewModel;
using VMM.Model;

namespace VMM.Content.View
{
    public partial class MusicListView : INotifyPropertyChanged
    {
        private MusicListViewModel _model;

        public MusicListView()
        {
            InitializeComponent();

            DataContext = Model;
            MusciView.SelectionChanged += MusciViewOnSelectionChanged;
        }

        public MusicListViewModel Model
        {
            get { return _model ?? (_model = new MusicListViewModel()); }
        }

        public MusicEntry[] SelectedItems
        {
            get
            {
                IEnumerable<MusicEntry> o = MusciView.SelectedItems.Cast<MusicEntry>();
                MusicEntry[] x = o.ToArray();
                return x;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void MusciViewOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            OnPropertyChanged("SelectedItems");
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            if (Model.Music.Count == 0 && Vk.Instance.LoggedIn)
            {
                Model.Refresh();
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