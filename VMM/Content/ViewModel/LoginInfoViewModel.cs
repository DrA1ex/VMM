using System;
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
    public class LoginInfoViewModel : INotifyPropertyChanged
    {
        private ICommand _changeAccountCommand;


        private bool _isBusy;

        private ICommand _retryCommand;

        public ICommand ChangeAccountCommand
        {
            get { return _changeAccountCommand ?? (_changeAccountCommand = new DelegateCommand(ChangeAccount)); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public ICommand RetryCommand
        {
            get { return _retryCommand ?? (_retryCommand = new DelegateCommand(Retry)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ChangeAccount()
        {
            Vk.Instance.Logout();
        }

        private void Retry()
        {
            IsBusy = true;
            Dispatcher disp = Dispatcher.CurrentDispatcher;

            Task.Run(() =>
                     {
                         try
                         {
                             Vk.Instance.Authorize(Vk.Instance.AccessToken);
                         }
                         catch (Exception e)
                         {
                             Trace.WriteLine(String.Format("While retry authorization: {0}", e.Message));
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
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}