﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using JetBrains.Annotations;
using VMM.Helper;
using VMM.Model;

namespace VMM.Content.ViewModel
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private bool _isBusy;
        private ICommand _loginCommand;
        public Action<string> AuthorizationFailed = s => { };
        public Action AuthorizationSuccess = () => { };
        public Func<string> GetPasswordMethod = () => null;

        public ICommand LoginCommand => _loginCommand ?? (_loginCommand = new DelegateCommand(Login));

        public string Email { get; set; }

        public bool ReadOnlyAccess { get; set; }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Login()
        {
            IsBusy = true;
            var disp = Dispatcher.CurrentDispatcher;

            Task.Run(() =>
            {
                try
                {
                    var results = Vk.Instance.Authorize(Email, GetPasswordMethod(), ReadOnlyAccess);

                    if(results.Success)
                    {
                        disp.Invoke(() => AuthorizationSuccess());
                    }
                    else
                    {
                        disp.Invoke(() => AuthorizationFailed(results.Message));
                    }
                }
                catch(Exception e)
                {
                    Trace.WriteLine(string.Format("While logging in: {0}", e));
                }
                finally
                {
                    disp.Invoke(() => { IsBusy = false; });
                }
            });
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}