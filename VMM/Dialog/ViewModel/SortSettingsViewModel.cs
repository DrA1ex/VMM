using System;
using System.Collections.Generic;
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
        private ICommand _romoveSortingPathCommand;
        private ObservableCollection<SortingPath> _selectedPaths;
        private SortingPath _selectedSortingPath;
        private ObservableCollection<SortingPath> _sortingPaths;

        public SortSettingsViewModel()
        {
            IEnumerable<SortingPath> paths = ReflectionHelper.GetStaticProperties(typeof(SortingPath)).OfType<SortingPath>();
            foreach (SortingPath path in paths)
            {
                OriginalSortingPathColeCollection.Add(path);
                SortingPaths.Add(path);
            }

            PrimarySortingPath = SortingPaths.First();
        }

        public ObservableCollection<SortingPath> SortingPaths
        {
            get { return _sortingPaths ?? (_sortingPaths = new ObservableCollection<SortingPath>()); }
        }

        public ObservableCollection<SortingPath> SelectedPaths
        {
            get { return _selectedPaths ?? (_selectedPaths = new ObservableCollection<SortingPath>()); }
        }

        private ObservableCollection<SortingPath> _originalSortingPathColeCollection;

        public ObservableCollection<SortingPath> OriginalSortingPathColeCollection
        {
            get { return _originalSortingPathColeCollection ?? (_originalSortingPathColeCollection = new ObservableCollection<SortingPath>()); }
        }


        private SortingPath _primarySortingPath;

        public SortingPath PrimarySortingPath
        {
            get { return _primarySortingPath; }
            set
            {
                if (_primarySortingPath != null)
                    SortingPaths.Add(_primarySortingPath);

                _primarySortingPath = value;
                SortingPaths.Remove(value);
                SelectedSortingPath = SortingPaths.FirstOrDefault();

                OnPropertyChanged("PrimarySortingPath");
            }
        }

        public SortingPath SelectedSortingPath
        {
            get { return _selectedSortingPath; }
            set
            {
                _selectedSortingPath = value;
                OnPropertyChanged("SelectedSortingPath");
            }
        }

        public ICommand InvertSortDirectionCommand
        {
            get { return _invertSortDirectionCommand ?? (_invertSortDirectionCommand = new DelegateCommand<SortingPath>(InvertSortDirection)); }
        }

        public ICommand AddNewSortingPathCommand
        {
            get { return _addNewSortingPathCommand ?? (_addNewSortingPathCommand = new DelegateCommand<SortingPath>(AddNewSortingPath)); }
        }

        public ICommand RomoveSortingPathCommand
        {
            get { return _romoveSortingPathCommand ?? (_romoveSortingPathCommand = new DelegateCommand<SortingPath>(RomoveSortingPath)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RomoveSortingPath(SortingPath path)
        {
            SelectedPaths.Remove(path);
            SortingPaths.Add(path);

            SelectedSortingPath = path;

        }

        private void AddNewSortingPath(SortingPath path)
        {
            if (path != null)
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
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}