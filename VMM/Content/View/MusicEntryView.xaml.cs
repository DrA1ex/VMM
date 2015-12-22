using System.Windows;
using VMM.Content.ViewModel;

namespace VMM.Content.View
{
    public partial class MusicEntryView
    {
        public MusicEntryView()
        {
            InitializeComponent();

            ActionsDock.DataContext = Model;
        }

        public MusicEntryViewModel Model { get; } = new MusicEntryViewModel();
    }
}