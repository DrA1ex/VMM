﻿using System.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using VMM.Content.ViewModel;

namespace VMM.Content.View
{
    public partial class Login
    {
        private LoginViewModel _model;

        public Login()
        {
            InitializeComponent();

            Model.AuthorizationFailed = AuthorizationFailed;
            Model.AuthorizationSuccess = AuthorizationSuccess;
            Model.GetPasswordMethod = GetPasswordMethod;

            DataContext = Model;
        }

        public LoginViewModel Model => _model ?? (_model = new LoginViewModel());

        private string GetPasswordMethod()
        {
            return PasswordBox.Password;
        }

        private void AuthorizationSuccess()
        {
            PasswordBox.Password = string.Empty;
        }

        private void AuthorizationFailed(string s)
        {
            ModernDialog.ShowMessage(string.Format("Авторизация не удалась: {0}", s), "Ошибка входа", MessageBoxButton.OK);

            PasswordBox.Password = string.Empty;
        }
    }
}