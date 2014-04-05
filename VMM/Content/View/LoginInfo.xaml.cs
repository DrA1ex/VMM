﻿using VMM.Content.ViewModel;

namespace VMM.Content.View
{
    public partial class LoginInfo
    {
        private LoginInfoViewModel _model;

        public LoginInfo()
        {
            InitializeComponent();

            DataContext = Model;
        }

        public LoginInfoViewModel Model
        {
            get { return _model ?? (_model = new LoginInfoViewModel()); }
        }
    }
}