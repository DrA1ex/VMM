using System.Windows;
using VMM.Content.ViewModel;

namespace VMM.Content.View
{
    public partial class MusicEntryView
    {
        private MusicEntryViewModel _model;

        public MusicEntryView()
        {
            InitializeComponent();

            ActionsDock.DataContext = Model;
        }

        public MusicEntryViewModel Model => _model ?? (_model = new MusicEntryViewModel());
    }
}