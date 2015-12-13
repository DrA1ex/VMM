using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JetBrains.Annotations;
using VMM.Helper;
using VMM.Model;

namespace VMM.Dialog.ViewModel
{
    public class SortSettingsViewModel : INotifyPropertyChanged
    {
        private ICommand _addNewSortingPathCommand;
        private ICommand _invertSortDirectionCommand;

        private ObservableCollection<SortingPath> _originalSortingPathColeCollection;


        private SortingPath _primarySortingPath;
        private ICommand _removeSortingPathCommand;
        private ObservableCollection<SortingPath> _selectedPaths;
        private SortingPath _selectedSortingPath;
        private ObservableCollection<SortingPath> _sortingPaths;

        public SortSettingsViewModel()
        {
            var paths = ReflectionHelper.GetStaticProperties(typeof(SortingPath)).OfType<SortingPath>();
            foreach(var path in paths)
            {
                OriginalSortingPathColeCollection.Add(path);
                SortingPaths.Add(path);
            }

            PrimarySortingPath = SortingPaths.First();
        }

        public ObservableCollection<SortingPath> SortingPaths
            => _sortingPaths ?? (_sortingPaths = new ObservableCollection<SortingPath>());

        public ObservableCollection<SortingPath> SelectedPaths
            => _selectedPaths ?? (_selectedPaths = new ObservableCollection<SortingPath>());

        public ObservableCollection<SortingPath> OriginalSortingPathColeCollection
            => _originalSortingPathColeCollection ?? (_originalSortingPathColeCollection = new ObservableCollection<SortingPath>());

        public SortingPath PrimarySortingPath
        {
            get { return _primarySortingPath; }
            set
            {
                if(_primarySortingPath != null)
                    SortingPaths.Add(_primarySortingPath);

                _primarySortingPath = value;
                SortingPaths.Remove(value);
                SelectedSortingPath = SortingPaths.FirstOrDefault();

                OnPropertyChanged(nameof(PrimarySortingPath));
            }
        }

        public SortingPath SelectedSortingPath
        {
            get { return _selectedSortingPath; }
            set
            {
                _selectedSortingPath = value;
                OnPropertyChanged(nameof(SelectedSortingPath));
            }
        }

        public ICommand InvertSortDirectionCommand
            => _invertSortDirectionCommand ?? (_invertSortDirectionCommand = new DelegateCommand<SortingPath>(InvertSortDirection));

        public ICommand AddNewSortingPathCommand
            => _addNewSortingPathCommand ?? (_addNewSortingPathCommand = new DelegateCommand<SortingPath>(AddNewSortingPath));

        public ICommand RemoveSortingPathCommand
            => _removeSortingPathCommand ?? (_removeSortingPathCommand = new DelegateCommand<SortingPath>(RemoveSortingPath));

        public event PropertyChangedEventHandler PropertyChanged;

        private void RemoveSortingPath(SortingPath path)
        {
            SelectedPaths.Remove(path);
            SortingPaths.Add(path);

            SelectedSortingPath = path;
        }

        private void AddNewSortingPath(SortingPath path)
        {
            if(path != null)
            {
                SelectedPaths.Add(path);
                SortingPaths.Remove(path);

                SelectedSortingPath = SortingPaths.FirstOrDefault();
            }
        }

        private void InvertSortDirection(SortingPath path)
        {
            path.Descending = !path.Descending;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}