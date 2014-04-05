using System.Windows;
using System.Windows.Controls;
using VMM.Content.ViewModel;

namespace VMM.Content.View
{
    public partial class MusicEntryView
    {
        private MusicEntryViewModel _model;

        public MusicEntryViewModel Model
        {
            get { return _model ?? (_model = new MusicEntryViewModel()); }
        }

        public MusicEntryView()
        {
            InitializeComponent();

            Loaded += ControlLoaded;
        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext != Model)
            {
                //We receive data context from MusicListView, but we have another one
                Grid.DataContext = DataContext;
                DataContext = Model;
            }
        }
    }
}