using System.Windows;
using VMM.Content.ViewModel;
using VMM.Model;

namespace VMM.Content.View
{
    public partial class MusicListView
    {
        private MusicListViewModel _model;

        public MusicListView()
        {
            InitializeComponent();

            DataContext = Model;
        }

        public MusicListViewModel Model
        {
            get { return _model ?? (_model = new MusicListViewModel()); }
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            if (Model.Music.Count == 0 && Vk.Instance.LoggedIn)
            {
                Model.Refresh();
            }
        }
    }
}