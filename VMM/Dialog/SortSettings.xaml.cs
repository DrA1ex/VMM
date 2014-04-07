using System.Linq;
using System.Windows;
using VMM.Dialog.ViewModel;
using VMM.Model;

namespace VMM.Dialog
{
    public partial class SortSettings
    {
        private SortSettingsViewModel _model;

        public SortSettingsViewModel Model
        {
            get { return _model ?? (_model = new SortSettingsViewModel()); }
        }

        public SortSettings()
        {
            InitializeComponent();

            DataContext = Model;
        }

        public SortingPath[] SortingPaths
        {
            get
            {
                return Model.SortingPaths.Any() ?
                    Model.SelectedPaths.ToArray() :
                    new[] { Model.PrimarySortingPath };
            }
        }

        private void OkClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Model.SelectedPaths.Insert(0, Model.PrimarySortingPath);
            Close();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}