﻿using System.Windows;
using System.Windows.Controls;
using VMM.Content.ViewModel;

namespace VMM.Content.View
{
    public partial class MusicListView : UserControl
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
            if (Model.Music.Count == 0)
            {
                Model.Refresh();
            }
        }
    }
}