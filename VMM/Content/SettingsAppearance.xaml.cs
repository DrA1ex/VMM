﻿using VMM.Content.ViewModel;

namespace VMM.Content
{
    public partial class SettingsAppearance
    {
        private SettingsAppearanceViewModel _viewModel;

        public SettingsAppearance()
        {
            InitializeComponent();

            DataContext = ViewModel;
        }

        public SettingsAppearanceViewModel ViewModel => _viewModel ?? (_viewModel = new SettingsAppearanceViewModel());
    }
}